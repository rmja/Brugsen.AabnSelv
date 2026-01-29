import * as colors from "../../laesoe-cards";

import { IRouteViewModel, IRouter } from "@aurelia/router";
import { resolve } from "aurelia";

import { IApiClient } from "../../api";

export class ApproveMember implements IRouteViewModel {
  private memberId!: string;
  email!: string;
  name!: string;
  address!: string;
  phone!: string;
  coopMembershipNumber!: string;
  laesoeCardNumber?: string;
  laesoeCardName?: string;
  laesoeCardColorClass?: string;

  constructor(
    private readonly api: IApiClient = resolve(IApiClient),
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
    if (cardColor) {
      this.laesoeCardName = colors[cardColor].name;
      this.laesoeCardColorClass = colors[cardColor].colorClass;
    }
  }

  async delete() {
    try {
      await this.api.delete(this.memberId).send();
      alert("Anmodningen er slettet");
      await this.router.load("../../dashboard", { context: this });
    } catch {
      alert("Kunne ikke slette anmodningen");
    }
  }

  async approve() {
    try {
      await this.api.approve(this.memberId).send();
      alert("Brugeren har nu adgang");
      await this.router.load("../../dashboard", { context: this });
    } catch {
      alert("Kunne ikke godkende anmodningen");
    }
  }
}
