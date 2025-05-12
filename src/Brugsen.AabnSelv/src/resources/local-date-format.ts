import { DateTime } from "luxon";
import { valueConverter } from "aurelia";

export type Kind = "short" | "med";

const map: { [kind in Kind]: Intl.DateTimeFormatOptions } = {
  short: DateTime.DATE_SHORT,
  med: DateTime.DATE_MED,
};

@valueConverter("localDate")
export class LocalDateValueConverter {
  toView(value: DateTime, kind?: Kind) {
    if (value && value.isValid) {
      const format = map[kind || "med"];
      return value.toLocal().toLocaleString(format);
    }
  }
}
