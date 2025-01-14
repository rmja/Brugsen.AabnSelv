import "@utiliread/http/json";

import { Http, Message } from "@utiliread/http";

import { LoginRedirectKey } from "./auth-handler";
import { UserManager } from "oidc-client-ts";
import { resolve } from "aurelia";

const anonymousHttp = new Http({ baseUrl: "api" });
const http = new Http({ baseUrl: "api" });

export interface Member {
  id: string;
  email: string;
  name: string;
  address: string;
  phone: string;
  coopMembershipNumber: number;
  laesoeCardNumber: number;
  laesoeCardColor: "red" | "blue" | "green";
  isApproved: boolean;
}

export type MemberInit = Omit<Member, "id" | "isApproved">;

export class ApiClient {
  constructor(private readonly userManager = resolve(UserManager)) {
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
}
