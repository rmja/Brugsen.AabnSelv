import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { ApiClient } from "../api";
import template from "./list.html";

@customElement({ name: "list-page", template })
export class ListPage implements IRouteableComponent {
  pending!: MemberViewModel[];

  constructor(
    private readonly api = resolve(ApiClient),
    private router = resolve(IRouter),
  ) {}

  async loading() {
    this.pending = await this.api.getPendingApproval().transfer();
  }

  approve(memberId: string) {
    return this.router.load("../approve", {
      parameters: { memberId },
      context: this,
    });
  }
}

interface MemberViewModel {
  id: string;
  name: string;
  email: string;
}
