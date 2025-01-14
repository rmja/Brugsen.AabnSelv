import Aurelia, { LoggerConfiguration, Registration } from "aurelia";

import { AppRootCustomElement } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { UserManager } from "oidc-client-ts";
import { ValuesValueConverter } from "./resources/values-value-converter";
import config from "./config";

const aurelia = new Aurelia()
  .register(RouterConfiguration.customize({ useUrlFragmentHash: false }))
  .register(LoggerConfiguration.create())
  .register(
    IntlTelInputConfiguration.customize({
      initialCountry: "dk",
      onlyCountries: ["dk", "se", "no", "de"],
    }),
    ValuesValueConverter,
    QrCodeCustomElement,
  )
  .register(
    Registration.instance(
      UserManager,
      new UserManager({
        authority: "_",
        client_id: config.client_id,
        redirect_uri: config.redirect_uri,
        response_type: "code",
        scope: "full_read_write offline",
        metadata: {
          authorization_endpoint: config.authorization_endpoint,
          token_endpoint: config.token_endpoint,
        },
      }),
    ),
  )
  .app({
    component: AppRootCustomElement,
    host: document.querySelector("app-root")!,
  });

await aurelia.start();
