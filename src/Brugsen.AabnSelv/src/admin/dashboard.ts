import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { ApiClient } from "../api";
import { DateTime } from "luxon";
import template from "./dashboard.html";

@customElement({ name: "dashboard-page", template })
export class DashboardPage implements IRouteableComponent {
  pending!: MemberViewModel[];
  alarmEvents!: ActionEventViewModel[];
  accessActivity!: AccessActivityViewModel[];

  constructor(
    private readonly api = resolve(ApiClient),
    private router = resolve(IRouter)
  ) {}

  async loading() {
    [this.pending, this.alarmEvents, this.accessActivity] = await Promise.all([
      this.api.getPendingApproval().transfer(),
      this.api.getActionEvents("alarm").transfer(),
      this.api.getAccessActivity().transfer(),
    ]);
  }

  approve(memberId: string) {
    return this.router.load(`../members/${memberId}/approve`, {
      context: this,
    });
  }

  getDuration(activity: AccessActivityViewModel) {
    if (activity.checkInEvent && activity.checkOutEvent) {
      return activity.checkInEvent.createdAt
        .until(activity.checkOutEvent.createdAt)
        .toDuration()
        .toFormat("mm:ss");
    }
  }

  async exportMembers() {
    const members = await this.api.getApproved().transfer();

    const rows = [
      [
        "Navn",
        "Adresse",
        "Coop medlemsnummer",
        "Ø-kort nummer",
        "Ø-kort farve",
        "Email",
      ],
      ...members.map((x) => [
        x.name,
        x.address,
        x.coopMembershipNumber,
        x.laesoeCardNumber,
        x.laesoeCardColor,
        x.email,
      ]),
    ];
    const BOM = "\uFEFF"; // BOM for UTF-8
    const csvContent =
      "data:text/csv;charset=utf-8," +
      BOM +
      rows.map((e) => e.join(";")).join("\n");

    window.open(encodeURI(csvContent));
  }
}

interface MemberViewModel {
  id: string;
  name: string;
  email: string;
}

interface ActionEventViewModel {
  action: string;
  method: string | null;
  createdAt: DateTime;
}

interface AccessActivityViewModel {
  memberName: string;
  checkInEvent: ActionEventViewModel | null;
  checkOutEvent: ActionEventViewModel | null;
}
