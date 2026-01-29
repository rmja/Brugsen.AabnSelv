import { IRouteViewModel, route } from "@aurelia/router";

import { customElement } from "aurelia";

@route({
  routes: [
    {
      path: "sales",
      component: () => import("./sales-report"),
      title: "Salgsrapport",
    },
  ],
})
@customElement({
  name: "reports-router",
  template: "<au-viewport></au-viewport>",
})
export class ReportsRouter implements IRouteViewModel {}
