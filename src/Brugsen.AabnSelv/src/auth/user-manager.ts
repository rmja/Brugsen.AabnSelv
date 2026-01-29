import { DI } from "aurelia";
import { UserManager } from "oidc-client-ts";
import config from "../config";

export const IUserManager = DI.createInterface<IUserManager>(
  "IUserManager",
  (x) =>
    x.instance(
      new UserManager({
        authority: "_",
        client_id: config.client_id,
        redirect_uri: config.redirect_uri,
        response_type: "code",
        scope: "full_read_write offline",
        metadata: {
          authorization_endpoint: config.authorization_endpoint,
          token_endpoint: config.token_endpoint,
        },
      }),
    ),
);
export type IUserManager = Required<UserManager>;
