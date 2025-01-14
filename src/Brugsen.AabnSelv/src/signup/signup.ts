import * as colors from "../laesoe-cards";

import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, observable, resolve } from "aurelia";

import { ApiClient } from "../api";
import { HttpError } from "@utiliread/http";
import template from "./signup.html";

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const membershipNumberRegex = /^[0-9]{8}$/;
const cardNumberRegex = /^[0-9]{10}$/;

type CardColor = "red" | "blue" | "green";

@customElement({ name: "signup-page", template })
export class SignupPage implements IRouteableComponent {
  busy = false;
  name = "Rasmus Jacobsen";
  address = "Plantagevej 30";
  @observable() email = "rmja@laesoe.org";
  emailIsUsed = false;
  phone = "+4520285909";
  membershipNumber = "12312312";
  @observable() cardNumber = "1231231231";
  cardNumberIsUsed = false;
  cardColor: CardColor = "red";
  colors: Record<CardColor, CardColorViewModel> = colors;
  termsAccepted = false;

  get canSubmit() {
    return (
      this.name.length > 5 &&
      this.name.trim().includes(" ") &&
      this.address.length > 5 &&
      this.address.trim().includes(" ") &&
      emailRegex.test(this.email) &&
      this.phone &&
      membershipNumberRegex.test(this.membershipNumber) &&
      cardNumberRegex.test(this.cardNumber) &&
      this.termsAccepted
    );
  }

  constructor(
    private api = resolve(ApiClient),
    private router: IRouter = resolve(IRouter),
  ) {}

  emailChanged() {
    this.emailIsUsed = false;
  }

  cardNumberChanged() {
    this.cardNumberIsUsed = false;
  }

  setColor(color: CardColor) {
    this.cardColor = color;
  }

  async submit() {
    this.busy = true;
    try {
      const member = await this.api
        .signup({
          name: this.name.trim(),
          address: this.address.trim(),
          email: this.email,
          phone: this.phone,
          coopMembershipNumber: Number(this.membershipNumber),
          laesoeCardNumber: Number(this.cardNumber),
          laesoeCardColor: this.cardColor,
        })
        .transfer();
      await this.router.load("../store-confirmation", {
        parameters: { memberId: member.id },
        context: this,
      });
    } catch (error) {
      if (error instanceof HttpError && error.hasDetails) {
        const details = await error.details();
        if (details.type.endsWith("/email-conflict")) {
          this.emailIsUsed = true;
        } else if (details.type.endsWith("/laesoe-card-number-conflict")) {
          this.cardNumberIsUsed = true;
        }
        return;
      }

      throw error;
    } finally {
      this.busy = false;
    }
  }
}

interface CardColorViewModel {
  id: CardColor;
  name: string;
  colorClass: string;
}
