import "@utiliread/http/json";

import { Http } from "@utiliread/http";

const http = new Http();

export interface Signup {
  email: string;
  name: string;
  address: string;
  phone: string;
}

export class ApiClient {
  public signup(signup: Signup) {
    return http.post("/signup").withJson(signup);
  }
}
