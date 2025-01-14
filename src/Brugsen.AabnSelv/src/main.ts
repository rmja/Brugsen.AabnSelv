import Aurelia, { LoggerConfiguration, Registration } from "aurelia";

import { AppRootCustomElement } from "./app-root";
import { IntlTelInputConfiguration } from "./resources/intl-tel-input";
import { QrCodeCustomElement } from "./resources/qr-code";
import { RouterConfiguration } from "@aurelia/router";
import { UserManager } from "oidc-client-ts";
import { ValuesValueConverter } from "./resources/values-value-converter";

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
        authority: "/oauth2",
        client_id: "app_41e92a4tuz9qpkykna91",
        redirect_uri: "http://localhost:60900/signin-oidc",
        response_type: "code",
        scope: "full_read_write offline",
        metadata: {
          authorization_endpoint: "http://localhost:60900/oauth2/auth",
          token_endpoint: "http://localhost:60900/oauth2/token",
        },
      }),
    ),
  )
  .app({
    component: AppRootCustomElement,
    host: document.querySelector("app-root")!,
  });

await aurelia.start();
