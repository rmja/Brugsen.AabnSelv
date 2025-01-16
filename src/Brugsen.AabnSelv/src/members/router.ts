import { IRouteableComponent, routes } from "@aurelia/router";

import { customElement } from "aurelia";
import template from "./router.html";

@routes([
  {
    path: "",
    redirectTo: "dashboard",
  },
  {
    id: "dashboard",
    path: "dashboard",
    component: import("./dashboard"),
    title: "Medlemmer",
  },
  {
    id: "approve",
    path: "approve",
    component: import("./approve"),
    title: "Godkendelse",
  },
])
@customElement({ name: "members-router", template })
export class MembersRouter implements IRouteableComponent {}
