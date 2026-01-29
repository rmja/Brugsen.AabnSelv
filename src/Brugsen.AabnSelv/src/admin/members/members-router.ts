import { IRouteViewModel, route } from "@aurelia/router";

import { customElement } from "aurelia";

@route({
  routes: [
    {
      path: ":memberId/approve",
      component: () => import("./approve-member"),
      title: "Godkendelse",
    },
  ],
})
@customElement({
  name: "members-router",
  template: "<au-viewport></au-viewport>",
})
export class MembersRouter implements IRouteViewModel {}
