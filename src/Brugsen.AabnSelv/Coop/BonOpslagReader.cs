using System.Globalization;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Brugsen.AabnSelv.Coop;

public class BonOpslagReader : IDisposable
{
    private readonly PdfDocument _document;
    private State _state = State.None;
    private DateOnly _date;
    private TimeOnly _time;
    private int _number = -1;
    private List<Line> _lines = [];
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

        switch (_state)
        {
            case State.ReadingBonHeader + 1:
                var date = DateOnly.ParseExact(
                    word.Text,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture
                );
                if (date != _date)
                {
                    _number = -1;
                    _date = date;
                }
                _state++;
                break;
            case State.ReadingBonHeader + 2:
                _time = TimeOnly.ParseExact(word.Text, "HH:mm", CultureInfo.InvariantCulture);
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
                var number = int.Parse(word.Text);
                if (_lineCategory is not null && number != _number)
                {
                    EmitBon();
                }
                else if (number == _number) { }
                _number = number;
                _state++;
                break;
            case State.ReadingPageFooter:
                break;
            case State.ReadingBonLines:

                if (word.BoundingBox.Left < 70)
                {
                    if (_lineCategory is not null)
                    {
                        _lines.Add(
                            new()
                            {
                                Category = _lineCategory!,
                                Text = _lineText.ToString(),
                                Amount = _lineAmount
                            }
                        );
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
        _lines.Add(
            new()
            {
                Category = _lineCategory!,
                Text = _lineText.ToString(),
                Amount = _lineAmount
            }
        );
        _bons.Add(
            new()
            {
                Purchased = _date.ToDateTime(_time, DateTimeKind.Unspecified),
                Number = _number,
                Lines = _lines
            }
        );

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
        ReadingBonLines = ReadingBonHeader + 25,
        ReadingPageFooter,
    }
}

public record Bon
{
    public required DateTime Purchased { get; init; }
    public int Number { get; init; }
    public required List<Line> Lines { get; init; }
}

public record Line
{
    public required string Category { get; init; }
    public required string Text { get; init; }
    public decimal? Amount { get; init; }
}
