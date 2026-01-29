import "@utiliread/http/json";

import { Http, HttpBuilderOfT, Message } from "@utiliread/http";
import { dateTimeConverter, jsonProperty } from "@utiliread/json";

import { DateTime } from "luxon";
import { LoginRedirectKey } from "./auth/auth-handler";
import { DI, resolve } from "aurelia";
import { IUserManager } from "./auth/user-manager";

const anonymousHttp = new Http({ baseUrl: "api" });
const http = new Http({ baseUrl: "api" });

export interface Member {
  id: string;
  email: string;
  name: string;
  address: string;
  phone: string;
  coopMembershipNumber: string;
  laesoeCardNumber?: string;
  laesoeCardColor?: "red" | "blue" | "green";
  isApproved: boolean;
}

export class ActionEvent<T = string> {
  @jsonProperty()
  action!: T;
  @jsonProperty()
  method!: "app" | "pin" | "nfc" | null;
  @jsonProperty({ converter: dateTimeConverter })
  createdAt!: DateTime;
}

export class AccessActivity {
  @jsonProperty()
  memberId!: string;
  @jsonProperty()
  memberName!: string;
  @jsonProperty()
  coopMembershipNumber!: string;
  @jsonProperty({ type: ActionEvent })
  checkInEvent!: ActionEvent<"check_in"> | null;
  @jsonProperty({ type: ActionEvent })
  checkOutEvent!: ActionEvent<"check_out" | "open_once"> | null;
}

export class Slip {
  @jsonProperty({ converter: dateTimeConverter })
  purchased!: DateTime;
  @jsonProperty()
  number!: number;
  @jsonProperty()
  lines!: { category: string; text: string; amount: number }[];
}

export class SalesReportLine {
  @jsonProperty()
  memberId!: string;
  @jsonProperty()
  memberName!: string;
  @jsonProperty()
  coopMembershipNumber!: string;
  @jsonProperty({ converter: dateTimeConverter })
  checkedInAt!: DateTime;
  @jsonProperty({ converter: dateTimeConverter })
  checkedOutAt!: DateTime;
  @jsonProperty({ type: Slip })
  slips!: Slip[];
  @jsonProperty()
  totalAmount!: number;
}

export class SalesReport {
  @jsonProperty() firstDate!: DateTime;
  @jsonProperty() lastDate!: DateTime;
  @jsonProperty({ type: SalesReportLine })
  lines!: SalesReportLine[];
}

export type AlarmAction = "arm" | "disarm";

export type MemberInit = Omit<Member, "id" | "isApproved">;

export const IApiClient = DI.createInterface<IApiClient>("IApiClient", (x) =>
  x.singleton(ApiClient),
);
export type IApiClient = Required<ApiClient>;

export class ApiClient {
  constructor(private readonly userManager = resolve(IUserManager)) {
    http.onSend(this.setAccessToken.bind(this));
  }

  private async setAccessToken(message: Message) {
    let user = await this.userManager.getUser();
    if (!user || user.expired) {
      await this.userManager.signinRedirect({
        state: {
          [LoginRedirectKey]: location.pathname + location.search,
        },
      });
      return;
    }

    message.headers.set("Authorization", `Bearer ${user.access_token}`);
  }

  signup(member: MemberInit) {
    return anonymousHttp
      .post("/members/signup")
      .withJson(member)
      .expectJson<Member>();
  }

  get(memberId: string) {
    return http.get(`/members/${memberId}`).expectJson<Member>();
  }

  getApproved() {
    return http.get(`/members/approved`).expectJson<Member[]>();
  }

  getPendingApproval() {
    return http.get(`/members/pending-approval`).expectJson<Member[]>();
  }

  getIsApproved(memberId: string) {
    return anonymousHttp
      .get(`/members/${memberId}/is-approved`)
      .expectJson<boolean>();
  }

  approve(memberId: string) {
    return http.post(`/members/${memberId}/approve`);
  }

  delete(memberId: string) {
    return http.delete(`/members/${memberId}`);
  }

  getAccessActivity(params?: { notBefore?: DateTime }) {
    return http
      .get(`/history/access-activity`, params)
      .expectJsonArray(AccessActivity);
  }

  getActionEvents(gadget: "alarm"): HttpBuilderOfT<ActionEvent<AlarmAction>[]>;
  getActionEvents<TAction>(gadget: string) {
    return http
      .get(`/history/${gadget}-action-events`)
      .expectJsonArray(ActionEvent<TAction>);
  }

  getSalesReport(model: { coopBonOpslag: Blob }) {
    const form = new FormData();
    form.set("coopBonOpslag", model.coopBonOpslag);
    return http.post("/reports/sales").withForm(form).expectJson(SalesReport);
  }
}
