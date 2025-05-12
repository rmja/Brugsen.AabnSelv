import { HttpError, statusCodes } from "@utiliread/http";
import { IRouteableComponent, IRouter } from "@aurelia/router";
import { customElement, resolve } from "aurelia";

import { IApiClient } from "../api";
import template from "./store-confirmation.html";

@customElement({ name: "store-confirmation-page", template })
export class StoreConfirmationPage implements IRouteableComponent {
  private memberId!: string;
  mode = __MODE__;
  approveRelativeUrl!: string;
  approveAbsoluteUrl!: string;

  constructor(
    private api: IApiClient = resolve(IApiClient),
    private router: IRouter = resolve(IRouter),
  ) {}

  loading(params: { memberId: string }) {
    this.memberId = params.memberId;

    let baseUrl = document.baseURI;
    if (baseUrl.endsWith("/")) {
      baseUrl = baseUrl.substring(0, baseUrl.length - 1);
    }

    this.approveRelativeUrl = `/admin/members/${params.memberId}/approve`;
    this.approveAbsoluteUrl = `${baseUrl}${this.approveRelativeUrl}`;
  }

  async verifyApproved() {
    try {
      const isApproved = await this.api.getIsApproved(this.memberId).transfer();
      if (!isApproved) {
        alert("Afventer godkendelse fra personalet");
        return;
      }

      await this.router.load("../receipt", { context: this });
    } catch (error) {
      if (
        error instanceof HttpError &&
        error.statusCode === statusCodes.status404NotFound
      ) {
        alert("Anmodningen er afvist");
        await this.router.load("../", { context: this });
      }
    }
  }
}
