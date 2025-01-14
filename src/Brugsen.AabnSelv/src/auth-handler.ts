import {
  IRouteableComponent,
  IRoutingInstruction,
  LoadInstruction,
  Navigation,
  RoutingInstruction,
} from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { UserManager } from "oidc-client-ts";
import config from "./config";

export const LoginRedirectKey = "loginRedirect";

@customElement("auth-handler")
export class AuthHandler implements IRouteableComponent {
  private readonly userManager = resolve(UserManager);

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
    return loginRedirect;
  }
}
