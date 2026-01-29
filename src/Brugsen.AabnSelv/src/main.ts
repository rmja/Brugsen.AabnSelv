import Aurelia, { LoggerConfiguration, LogLevel } from "aurelia";

import { AppRoot } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { ValuesValueConverter } from "./resources/values-value-converter";
import { Settings } from "luxon";
import { LocalDateTimeValueConverter } from "./resources/local-date-time-format";
import { I18nConfiguration } from "@aurelia/i18n";
import { DawaAutocompleteCustomAttribute } from "./resources/dawa-autocomplete";
import { LocalDateValueConverter } from "./resources/local-date-format";
import * as da from "./locales/da.json";

(<any>Symbol).metadata ??= Symbol("Symbol.metadata");
Settings.defaultLocale = "da-DK";
Settings.throwOnInvalid = true;

await Aurelia.register(
  RouterConfiguration.customize({
    useUrlFragmentHash: false,
  }),
)
  .register(LoggerConfiguration.create({ level: LogLevel.trace }))
  .register(
    IntlTelInputConfiguration.customize({
      initialCountry: "dk",
      onlyCountries: ["dk", "se", "no", "de"],
    }),
    ValuesValueConverter,
    QrCodeCustomElement,
    LocalDateValueConverter,
    LocalDateTimeValueConverter,
    DawaAutocompleteCustomAttribute,
    I18nConfiguration.customize(
      (options) =>
        (options.initOptions = {
          lng: "da",
          resources: {
            da: { translation: da },
          },
        }),
    ),
  )
  .app(AppRoot)
  .start();
