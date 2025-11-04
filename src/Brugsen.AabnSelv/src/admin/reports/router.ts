import { IRouteViewModel, route } from "@aurelia/router";

import { customElement } from "aurelia";

@route({
  routes: [
    {
      id: "sale",
      path: "sale",
      component: import("./sale"),
      title: "Salgsrapport",
    },
  ],
})
@customElement({
  name: "admin-router",
  template: "<au-viewport></au-viewport>",
})
export class AdminRouter implements IRouteViewModel {}
