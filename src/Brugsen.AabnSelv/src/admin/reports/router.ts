import { IRouteableComponent, routes } from "@aurelia/router";

import { customElement } from "aurelia";

@routes([
  {
    id: "sale",
    path: "sale",
    component: import("./sale"),
    title: "Salgsrapport",
  }
])
@customElement({ name: "admin-router", template: "<au-viewport></au-viewport>" })
export class AdminRouter implements IRouteableComponent {}
