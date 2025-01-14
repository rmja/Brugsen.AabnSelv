import { customElement, resolve } from "aurelia";

import { IRouteableComponent } from "@aurelia/router";
import { UserManager } from "oidc-client-ts";

@customElement("auth-handler")
export class AuthHandler implements IRouteableComponent {
  private readonly userManager = resolve(UserManager);

  async canLoad() {
    await this.userManager.signinCallback();
    return true;
  }
}
