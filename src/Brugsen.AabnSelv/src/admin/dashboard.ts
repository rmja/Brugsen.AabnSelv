import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { ApiClient } from "../api";
import { DateTime } from "luxon";
import template from "./dashboard.html";

@customElement({ name: "dashboard-page", template })
export class DashboardPage implements IRouteableComponent {
  pending!: MemberViewModel[];
  lockEvents!: EventViewModel[];
  alarmEvents!: EventViewModel[];
  accessActivity!: AccessActivityViewModel[];

  constructor(
    private readonly api = resolve(ApiClient),
    private router = resolve(IRouter),
  ) {}

  async loading() {
    [this.pending, this.lockEvents, this.alarmEvents, this.accessActivity] =
      await Promise.all([
        this.api.getPendingApproval().transfer(),
        this.api.getEvents("front-door-lock").transfer(),
        this.api.getEvents("alarm").transfer(),
        this.api.getAccessActivity().transfer(),
      ]);
  }

  approve(memberId: string) {
    return this.router.load(`../members/${memberId}/approve`, {
      context: this,
    });
  }

  getDuration(activity: AccessActivityViewModel) {
    if (activity.checkedInAt && activity.checkedOutAt) {
      return activity.checkedInAt
        .until(activity.checkedOutAt)
        .toDuration()
        .toFormat("mm:ss");
    }
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

interface AccessActivityViewModel {
  memberName: string;
  checkedInAt?: DateTime;
  checkedOutAt?: DateTime;
}
