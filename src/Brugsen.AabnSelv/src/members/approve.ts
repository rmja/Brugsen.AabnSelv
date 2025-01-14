import * as colors from "../laesoe-cards";

import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { ApiClient } from "../api";
import template from "./approve.html";

@customElement({ name: "approve-page", template })
export class ApprovePage implements IRouteableComponent {
  private memberId!: string;
  email!: string;
  name!: string;
  address!: string;
  phone!: string;
  coopMembershipNumber!: number;
  laesoeCardNumber!: number;
  laesoeCardName!: string;
  laesoeCardColorClass!: string;

  constructor(
    private readonly api: ApiClient = resolve(ApiClient),
    private readonly router = resolve(IRouter),
  ) {}

  async loading(params: { memberId: string }) {
    this.memberId = params.memberId;
    const member = await this.api.get(params.memberId).transfer();
    this.email = member.email;
    this.name = member.name;
    this.address = member.address;
    this.phone = member.phone;
    this.coopMembershipNumber = member.coopMembershipNumber;
    this.laesoeCardNumber = member.laesoeCardNumber;
    const cardColor = member.laesoeCardColor;
    this.laesoeCardName = colors[cardColor].name;
    this.laesoeCardColorClass = colors[cardColor].colorClass;
  }

  async delete() {
    try {
      await this.api.delete(this.memberId).send();
      alert("Anmodningen er slettet");
      await this.router.load("../list", { context: this });
    } catch {
      alert("Kunne ikke slette anmodningen");
    }
  }

  async approve() {
    try {
      await this.api.approve(this.memberId).send();
      await this.router.load("../list", { context: this });
    } catch {
      alert("Kunne ikke godkende anmodningen");
    }
  }
}
