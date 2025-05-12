import Aurelia, { LoggerConfiguration } from "aurelia";

import { AppRootCustomElement } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { ValuesValueConverter } from "./resources/values-value-converter";
import { Settings } from "luxon";
import { LocalDateTimeValueConverter } from "./resources/local-date-time-format";
import { I18nConfiguration } from "@aurelia/i18n";
import * as da from "./locales/da.json";
import { DawaAutocompleteCustomAttribute } from "./resources/dawa-autocomplete";
import { LocalDateValueConverter } from "./resources/local-date-format";

(<any>Symbol).metadata ??= Symbol("Symbol.metadata");
Settings.defaultLocale = "da-DK";
Settings.throwOnInvalid = true;

const aurelia = new Aurelia()
  .register(
    RouterConfiguration.customize({
      title: "${componentTitles}${appTitleSeparator}Brugsen Vesterø Havn",
      useUrlFragmentHash: false,
    }),
  )
  .register(LoggerConfiguration.create())
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
  .app({
    component: AppRootCustomElement,
    host: document.querySelector("app-root")!,
  });

await aurelia.start();
