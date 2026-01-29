import { IRouteViewModel, route } from "@aurelia/router";
import logoPath from "../assets/brugsen-vesteroe-havn-logo.png";

@route({
  routes: [
    {
      path: "",
      component: () => import("./contact-step"),
      title: "Brugeroprettelse",
    },
    {
      path: "store-confirmation",
      component: () => import("./store-confirmation-step"),
      title: "Butiksbekræftelse",
    },
    {
      path: "receipt",
      component: () => import("./receipt-step"),
      title: "Kvittering",
    },
  ],
})
export class SignupRouter implements IRouteViewModel {
  logoPath = logoPath;
}
