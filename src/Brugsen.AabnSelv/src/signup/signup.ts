import * as colors from "../laesoe-cards";

import { IRouteViewModel, IRouter, RouteNode } from "@aurelia/router";
import { customElement, observable, resolve } from "aurelia";

import { HttpError } from "@utiliread/http";
import { IApiClient } from "../api";
import template from "./signup.html";

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const membershipNumberRegex = /^[0-9]{6,8}$/;
const cardNumberRegex = /^[0-9]{7,10}$/;

type CardColor = "red" | "blue" | "green";

@customElement({ name: "signup-page", template })
export class SignupPage implements IRouteViewModel {
  busy = false;
  name = "";
  address = "";
  addressId: string | null = null;
  @observable() email = "";
  emailIsUsed = false;
  @observable() phone = "";
  phoneIsUsed = false;
  membershipNumber = "";
  @observable() cardNumber = "";
  cardNumberIsUsed = false;
  cardColor: CardColor = "red";
  colors: Record<CardColor, CardColorViewModel> = colors;
  gdprAccepted = false;
  termsAccepted = false;

  get nameIsValid() {
    return this.name.length > 5 && this.name.trim().includes(" ");
  }

  get addressIsValid() {
    return !!this.addressId;
  }

  get addressIsInvalid() {
    return this.address.length > 0 && !this.addressId;
  }

  get emailIsValid() {
    return emailRegex.test(this.email);
  }

  get phoneIsValid() {
    return !!this.phone;
  }

  get membershipNumberIsValid() {
    return membershipNumberRegex.test(this.membershipNumber);
  }

  get cardNumberIsValid() {
    return cardNumberRegex.test(this.cardNumber);
  }

  get canSubmit() {
    return (
      this.nameIsValid &&
      this.addressIsValid &&
      this.emailIsValid &&
      this.phoneIsValid &&
      this.membershipNumberIsValid &&
      this.cardNumberIsValid &&
      this.gdprAccepted &&
      this.termsAccepted
    );
  }

  constructor(
    private api = resolve(IApiClient),
    private router: IRouter = resolve(IRouter),
  ) {}

  emailChanged() {
    this.emailIsUsed = false;
  }

  phoneChanged() {
    this.phoneIsUsed = false;
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
          coopMembershipNumber: this.membershipNumber,
          laesoeCardNumber: this.cardNumber,
          laesoeCardColor: this.cardColor,
        })
        .transfer();
      await this.router.load("../store-confirmation", {
        queryParams: { memberId: member.id },
        context: this,
      });
    } catch (error) {
      if (error instanceof HttpError && error.hasDetails) {
        const details = await error.details();
        if (details.type.endsWith("/email-conflict")) {
          this.emailIsUsed = true;
        } else if (details.type.endsWith("/phone-conflict")) {
          this.phoneIsUsed = true;
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
