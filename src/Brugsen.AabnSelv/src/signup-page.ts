import { HttpError, statusCodes } from "@utiliread/http";
import { IRouteableComponent, IRouter, Router } from "@aurelia/router";
import { customElement, observable, resolve } from "aurelia";

import { ApiClient } from "./api";
import template from "./signup-page.html";

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@customElement({ name: "signup-page", template })
export class SignupPage implements IRouteableComponent {
  name = "Rasmus";
  address = "Plantagevej 30";
  @observable() email = "rmja@laesoe.org";
  emailIsUsed = false;
  phone = "+4520285909";
  termsAccepted = false;

  get canSubmit() {
    return (
      this.name &&
      this.address &&
      emailRegex.test(this.email) &&
      this.phone &&
      this.termsAccepted
    );
  }

  constructor(
    private api = resolve(ApiClient),
    private router: IRouter = resolve(IRouter)
  ) {}

  emailChanged() {
    this.emailIsUsed = false;
  }

  async submit() {
    try {
      await this.api
        .signup({
          name: this.name,
          address: this.address,
          email: this.email,
          phone: this.phone,
        })
        .send();

      this.router.load("/receipt");
    } catch (error) {
      if (
        error instanceof HttpError &&
        error.statusCode === statusCodes.status409Conflict
      ) {
        this.emailIsUsed = true;
      }
    }
  }
}
