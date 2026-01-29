import { customElement, resolve } from "aurelia";

import { IRouteViewModel } from "@aurelia/router";
import config from "../config";
import { IUserManager } from "./user-manager";

export const LoginRedirectKey = "loginRedirect";

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
