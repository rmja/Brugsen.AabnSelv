import "@utiliread/http/json";

import { Http, Message } from "@utiliread/http";
import { dateTimeConverter, jsonProperty } from "@utiliread/json";

import { DateTime } from "luxon";
import { IUserManager } from "./oauth";
import { LoginRedirectKey } from "./oauth";
import { resolve } from "aurelia";

const anonymousHttp = new Http({ baseUrl: "api" });
const http = new Http({ baseUrl: "api" });

export interface Member {
  id: string;
  email: string;
  name: string;
  address: string;
  phone: string;
  coopMembershipNumber: string;
  laesoeCardNumber: string;
  laesoeCardColor: "red" | "blue" | "green";
  isApproved: boolean;
}

export class AccessActivity {
  @jsonProperty()
  memberId!: string;
  @jsonProperty()
  memberName!: string;
  @jsonProperty({ converter: dateTimeConverter })
  checkedInAt?: DateTime;
  @jsonProperty({ converter: dateTimeConverter })
  checkedOutAt?: DateTime;
}

export class Event<T = string> {
  @jsonProperty()
  action!: T;
  @jsonProperty({ converter: dateTimeConverter })
  createdAt!: DateTime;
}

export type AlarmAction = "arm" | "disarm";

export type MemberInit = Omit<Member, "id" | "isApproved">;

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

  getAccessActivity() {
    return http
      .get(`/history/access-activity`)
      .expectJsonArray(AccessActivity);
  }

  getEvents(gadget: "alarm" | "front-door-lock") {
    return http.get(`/history/${gadget}-events`).expectJsonArray(Event<AlarmAction>);
  }
}
