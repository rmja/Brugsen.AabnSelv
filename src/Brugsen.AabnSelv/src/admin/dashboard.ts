import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { ApiClient } from "../api";
import { DateTime } from "luxon";
import template from "./dashboard.html";

@customElement({ name: "dashboard-page", template })
export class DashboardPage implements IRouteableComponent {
  pending!: MemberViewModel[];
  alarmEvents!: EventViewModel[];
  doorActivity!: DoorActivityViewModel[];

  constructor(
    private readonly api = resolve(ApiClient),
    private router = resolve(IRouter)
  ) {}

  async loading() {
    [this.pending, this.alarmEvents, this.doorActivity] = await Promise.all([
      this.api.getPendingApproval().transfer(),
      this.api.getEvents("alarm").transfer(),
      this.api.getFrontDoorActivity().transfer(),
    ]);
  }

  approve(memberId: string) {
    return this.router.load(`../members/${memberId}/approve`, {
      context: this,
    });
  }
}

interface MemberViewModel {
  id: string;
  name: string;
  email: string;
}

interface EventViewModel {
  action: string;
  createdAt: DateTime;
}

interface DoorActivityViewModel {
  memberName: string;
  enteredAt?: DateTime,
  exitedAt?: DateTime,
}
