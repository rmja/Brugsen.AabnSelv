import { IRouteViewModel, route } from "@aurelia/router";

import { customElement } from "aurelia";
import template from "./admin-router.html";

@route({
  routes: [
    {
      path: "",
      redirectTo: "dashboard",
    },
    {
      id: "dashboard",
      path: "dashboard",
      component: import("./admin-dashboard"),
      title: "Admin Dashboard",
    },
    {
      id: "members",
      path: "members",
      component: import("./members/members-router"),
      title: "Medlemmer",
    },
    {
      id: "reports",
      path: "reports",
      component: import("./reports/reports-router"),
      title: "Rapporter",
    },
  ],
})
@customElement({ name: "admin-router", template })
export class AdminRouter implements IRouteViewModel {}
