import { DI, customElement, resolve } from "aurelia";

import { IRouteViewModel } from "@aurelia/router";
import { UserManager } from "oidc-client-ts";
import config from "./config";

export const LoginRedirectKey = "loginRedirect";

export const IUserManager = DI.createInterface<IUserManager>(
  "IAuthService",
  (x) =>
    x.instance(
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
);
export type IUserManager = Required<UserManager>;

@customElement("auth-handler")
export class AuthHandler implements IRouteViewModel {
  constructor(private readonly userManager = resolve(IUserManager)) {}

  async canLoad() {
    const user = await this.userManager.signinCallback();

    let loginRedirect = config.login_redirect;
    if (
      user &&
      user.state &&
      typeof user.state === "object" &&
      LoginRedirectKey in user.state &&
      typeof user.state[LoginRedirectKey] === "string"
    ) {
      loginRedirect = user.state[LoginRedirectKey];
    }

    // const result: IRoutingInstruction = {
    //   component: loginRedirect,
    //   options: {
    //     replace: true,
    //   },
    // };
    window.location.assign(loginRedirect);

    // Does not work
    // return loginRedirect;

    return false;
  }
}
