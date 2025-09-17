import StatementInfo from "../Domain/StatementInfo";
import StatusRequest from "./StatusRequest";

export default interface StatementListPagedResult extends StatusRequest {
  statements: StatementInfo[];
  pageNum: number;
  pageCount: number;
}