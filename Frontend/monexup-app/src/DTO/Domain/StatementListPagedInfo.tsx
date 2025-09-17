import StatementInfo from "../Domain/StatementInfo";

export default interface StatementListPagedInfo {
  statements: StatementInfo[];
  pageNum: number;
  pageCount: number;
}