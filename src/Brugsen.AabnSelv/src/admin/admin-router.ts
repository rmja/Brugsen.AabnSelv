import { IRouteViewModel, route } from "@aurelia/router";
import logoPath from "../assets/brugsen-vesteroe-havn-logo.png";

@route({
  routes: [
    {
      path: "",
      redirectTo: "dashboard",
    },
    {
      
      path: "dashboard",
      component: () => import("./admin-dashboard"),
      title: "Admin Dashboard",
    },
    {
      
      path: "members",
      component: () => import("./members/members-router"),
      title: "Medlemmer",
    },
    {
      
      path: "reports",
      component: () => import("./reports/reports-router"),
      title: "Rapporter",
    },
  ],
})
export class AdminRouter implements IRouteViewModel {
  logoPath = logoPath;
}
