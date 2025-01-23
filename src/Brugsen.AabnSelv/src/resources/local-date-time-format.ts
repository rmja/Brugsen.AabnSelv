import { DateTime } from "luxon";
import { valueConverter } from "aurelia";

export type Kind = "short" | "med";

const map: { [kind in Kind]: Intl.DateTimeFormatOptions } = {
  short: DateTime.DATETIME_SHORT,
  med: DateTime.DATETIME_MED,
};

@valueConverter("localDateTime")
export class LocalDateTimeValueConverter {
  toView(value: DateTime, kind?: Kind) {
    if (value && value.isValid) {
      const format = map[kind || "med"];
      return value.toLocal().toLocaleString(format);
    }
  }
}
