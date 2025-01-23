import Aurelia, { LoggerConfiguration } from "aurelia";

import { AppRootCustomElement } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { ValuesValueConverter } from "./resources/values-value-converter";
import { Settings } from "luxon";
import { LocalDateTimeValueConverter } from "./resources/local-date-time-format";

(<any>Symbol).metadata ??= Symbol("Symbol.metadata");

Settings.defaultLocale = "da-DK";
Settings.throwOnInvalid = true;


declare module "luxon" {
  interface TSSettings {
    throwOnInvalid: true;
  }
}


const aurelia = new Aurelia()
  .register(
    RouterConfiguration.customize({
      title: "${componentTitles}${appTitleSeparator}Brugsen Vesterø Havn",
      useUrlFragmentHash: false,
    })
  )
  .register(LoggerConfiguration.create())
  .register(
    IntlTelInputConfiguration.customize({
      initialCountry: "dk",
      onlyCountries: ["dk", "se", "no", "de"],
    }),
    ValuesValueConverter,
    QrCodeCustomElement,
    LocalDateTimeValueConverter,
  )
  .app({
    component: AppRootCustomElement,
    host: document.querySelector("app-root")!,
  });

await aurelia.start();
