import Aurelia, { LoggerConfiguration, Registration } from "aurelia";

import { AppRootCustomElement } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { ValuesValueConverter } from "./resources/values-value-converter";

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
    QrCodeCustomElement
  )
  .app({
    component: AppRootCustomElement,
    host: document.querySelector("app-root")!,
  });

await aurelia.start();
