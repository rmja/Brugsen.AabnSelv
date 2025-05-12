using System.Globalization;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Brugsen.AabnSelv.Coop;

public sealed class BonOpslagReader : IDisposable
{
    private readonly PdfDocument _document;
    private State _state = State.None;

    private DateOnly _headerDate;
    private TimeOnly _headerTime;
    private int _headerNumber = -1;

    private DateOnly _date;
    private TimeOnly _time;
    private int _number = -1;
    private List<BonLine> _lines = [];
    private string? _lineCategory;
    private readonly StringBuilder _lineText = new();
    private decimal? _lineAmount;
    private List<Bon> _bons = [];

    public BonOpslagReader(Stream pdfStream)
    {
        _document = PdfDocument.Open(pdfStream);
    }

    public IEnumerable<Bon> ReadAsync()
    {
        if (_state != State.None)
        {
            throw new InvalidOperationException();
        }

        foreach (var page in _document.GetPages())
        {
            foreach (var word in page.GetWords())
            {
                ProcessWord(word);
            }
        }

        if (_lineCategory is not null)
        {
            EmitBon();
        }

        return _bons;
    }

    private void ProcessWord(Word word)
    {
        if (word.Text == "Kardex:")
        {
            _state = State.ReadingPageHeader;
        }
        else if (word.Text == "Kassedato:")
        {
            _state = State.ReadingBonHeader;
        }
        else if (word.Text == "bon_opslag")
        {
            _state = State.ReadingPageFooter;
        }
        else if (_state == State.ReadingBonHeader + 25 && word.Text != "Gemt:")
        {
            // "Gemt" in header only exists in some headers
            _state = State.ReadingBonLines;
        }

        switch (_state)
        {
            case State.ReadingBonHeader + 1:
                _headerDate = DateOnly.ParseExact(
                    word.Text,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
                _state++;
                break;
            case State.ReadingBonHeader + 2:
                _headerTime = TimeOnly.ParseExact(word.Text, "HH:mm", CultureInfo.InvariantCulture);
                _state++;
                break;
            case State.ReadingBonHeader + 12:
                if (word.Text != "Bonnr:")
                {
                    throw new InvalidDataException();
                }
                _state++;
                break;
            case State.ReadingBonHeader + 13:
                _headerNumber = int.Parse(word.Text);
                _state++;
                break;

            case State.ReadingPageFooter:
                break;
            case State.ReadingBonLines:
                if (_date == default)
                {
                    _date = _headerDate;
                    _time = _headerTime;
                    _number = _headerNumber;
                }
                else if (_date != _headerDate || _number != _headerNumber)
                {
                    EmitBon();
                }

                if (word.BoundingBox.Left < 70)
                {
                    if (_lineCategory is not null)
                    {
                        _lines.Add(new(_lineCategory!, _lineText.ToString(), _lineAmount));
                        _lineCategory = null;
                        _lineText.Clear();
                        _lineAmount = null;
                    }

                    _lineCategory = word.Text;
                }
                else if (word.BoundingBox.Left > 450)
                {
                    _lineAmount = decimal.Parse(
                        word.Text.Replace(',', '.'),
                        CultureInfo.InvariantCulture
                    );
                }
                else
                {
                    if (_lineText.Length > 0)
                    {
                        _lineText.Append(' ');
                    }
                    _lineText.Append(word.Text);
                }
                break;
            default:
                _state++;
                break;
        }
    }

    private void EmitBon()
    {
        _lines.Add(new(_lineCategory!, _lineText.ToString(), _lineAmount));
        _bons.Add(
            new()
            {
                Purchased = _date.ToDateTime(_time, DateTimeKind.Unspecified),
                Number = _number,
                Lines = _lines
            }
        );

        _date = default;
        _time = default;
        _number = -1;
        _lines = [];
        _lineCategory = null;
        _lineText.Clear();
        _lineAmount = null;
    }

    public void Dispose()
    {
        _document.Dispose();
    }

    enum State
    {
        None,
        ReadingPageHeader,
        ReadingBonHeader = ReadingPageHeader + 9,
        ReadingBonLines = ReadingBonHeader + 27,
        ReadingPageFooter,
    }
}
