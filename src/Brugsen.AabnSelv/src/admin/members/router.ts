import { IRouteableComponent, routes } from "@aurelia/router";

import { customElement } from "aurelia";

@routes([
  {
    id: "approve",
    path: ":memberId/approve",
    component: import("./approve"),
    title: "Godkendelse",
  },
])
@customElement({ name: "members-router", template: "<au-viewport></au-viewport>" })
export class MembersRouter implements IRouteableComponent {}
