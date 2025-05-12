import { customElement, resolve } from "aurelia";

import { DateTime } from "luxon";
import { IApiClient } from "../../api";
import template from "./sale.html";

@customElement({ name: "sale-page", template })
export class SalePage {
  fileName = "";
  fileInput!: HTMLInputElement;
  days!: DayViewModel[];
  totalAccessCount = 0;
  totalMemberCount = 0;
  totalSlipCount = 0;
  totalAmount = 0;
  isBusy = false;

  get canSubmit() {
    return this.fileName.length > 0;
  }

  constructor(private readonly api = resolve(IApiClient)) {}

  async getReport() {
    if (!this.canSubmit || this.isBusy) {
      return;
    }

    this.isBusy = true;
    const file = this.fileInput.files![0]!;
    const report = await this.api
      .getSalesReport({ coopBonOpslag: file })
      .transfer();

    const grouped = report.lines.reduce(
      (acc, line) => {
        const time = line.checkedInAt;
        const date = time.toLocal().startOf("day");
        const key = date.toFormat("yyyy-MM-dd");
        if (!acc[key]) {
          acc[key] = {
            date,
            accessCount: 0,
            members: new Set<string>(),
            slipCount: 0,
            amount: 0
          };
        }
        acc[key].accessCount++;
        acc[key].members.add(line.memberId);
        acc[key].slipCount += line.slips.length;
        acc[key].amount += line.totalAmount;
        return acc;
      },
      {} as Record<string, DayViewModel>
    );

    this.days = Object.values(grouped).sort((a, b) => +a.date.diff(b.date));
    this.totalAccessCount = this.days.reduce((acc, x) => acc + x.accessCount, 0);
    this.totalMemberCount = this.days.reduce((acc, x) => acc + x.members.size, 0);
    this.totalSlipCount = this.days.reduce((acc, x) => acc + x.slipCount, 0);
    this.totalAmount = this.days.reduce((acc, x) => acc + x.amount, 0);
    this.isBusy = false;
  }
}

interface DayViewModel {
  date: DateTime;
  accessCount: number;
  members: Set<string>;
  slipCount: number;
  amount: number;
}
