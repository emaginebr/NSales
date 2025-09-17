import { faCalendar, faDollar } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import Table from "react-bootstrap/esm/Table";
import StatementListPagedInfo from "../../DTO/Domain/StatementListPagedInfo";
import Moment from "react-moment";
import Row from "react-bootstrap/esm/Row";
import Col from "react-bootstrap/esm/Col";
import Pagination from 'react-bootstrap/Pagination';
import { useTranslation } from "react-i18next";

interface IStatementParam {
    loading: boolean;
    StatementResult: StatementListPagedInfo;
    onChangePage: (pageNum: number) => void;
}

export default function StatementPart(param: IStatementParam) {

    const { t } = useTranslation();

    return (
        <>
            <Table striped bordered hover>
                <thead>
                    <tr>
                        <th>-</th>
                        <th>{t('statement_pay_date')}</th>
                        <th>{t('statement_network')}</th>
                        <th>{t('statement_product')}</th>
                        <th>{t('statement_buyer')}</th>
                        <th>{t('statement_seller')}</th>
                        <th style={{ textAlign: "right" }}>{t('statement_amount')}</th>
                        <th>{t('statement_paid_at')}</th>
                    </tr>
                </thead>
                <tbody>
                    {param.loading &&
                        <tr>
                            <td colSpan={8}>
                                <div className="d-flex justify-content-center">
                                    <div className="spinner-border" role="status">
                                        <span className="visually-hidden">{t('loading')}</span>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    }
                    {!param.loading && param.StatementResult?.statements.map((statement) => {
                        return (
                            <tr>
                                <td className="text-success"><FontAwesomeIcon icon={faDollar} fixedWidth /></td>
                                <td><Moment format="DD/MM/YYYY">{statement.paymentDate}</Moment></td>
                                <td>{statement.networkName}</td>
                                <td>{statement.description}</td>
                                <td>{statement.buyerName}</td>
                                <td>{statement.sellerName}</td>
                                <td style={{ textAlign: "right" }}>R$ {statement.amount}</td>
                                <td>{statement.paidAt}</td>
                            </tr>
                        );
                    })}
                    {!param.loading && param.StatementResult?.statements.length == 0 &&
                        <tr>
                            <td colSpan={8} className="text-center">
                                {t('statement_no_statement_found')}
                            </td>
                        </tr>
                    }
                </tbody>
            </Table>
            {!param.loading && param.StatementResult &&
                <Row>
                    <Col md={12} className="text-center">
                        <Pagination className="justify-content-center">
                            <Pagination.First
                                disabled={!(param.StatementResult?.pageNum > 1)}
                                onClick={() => param.onChangePage(1)} />
                            <Pagination.Prev
                                disabled={!(param.StatementResult?.pageNum > 1)}
                                onClick={() => param.onChangePage(param.StatementResult?.pageNum - 1)} />
                            <Pagination.Ellipsis />

                            {param.StatementResult?.pageNum - 2 >= 1 &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.StatementResult?.pageNum - 2)}
                                >{param.StatementResult?.pageNum - 2}</Pagination.Item>
                            }
                            {param.StatementResult?.pageNum - 1 >= 1 &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.StatementResult?.pageNum - 1)}
                                >{param.StatementResult?.pageNum - 1}</Pagination.Item>
                            }
                            <Pagination.Item active>{param.StatementResult?.pageNum}</Pagination.Item>
                            {param.StatementResult?.pageNum + 1 <= param.StatementResult?.pageCount &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.StatementResult?.pageNum + 1)}
                                >{param.StatementResult?.pageNum + 1}</Pagination.Item>
                            }
                            {param.StatementResult?.pageNum + 2 <= param.StatementResult?.pageCount &&
                                <Pagination.Item
                                    onClick={() => param.onChangePage(param.StatementResult?.pageNum + 2)}
                                >{param.StatementResult?.pageNum + 2}</Pagination.Item>
                            }

                            <Pagination.Ellipsis />
                            <Pagination.Next
                                disabled={!(param.StatementResult?.pageNum < param.StatementResult?.pageCount)}
                                onClick={() => param.onChangePage(param.StatementResult?.pageCount)}
                            />
                            <Pagination.Last
                                disabled={!(param.StatementResult?.pageNum < param.StatementResult?.pageCount)}
                                onClick={() => param.onChangePage(param.StatementResult?.pageCount)} />
                        </Pagination>
                    </Col>
                </Row>
            }
        </>
    );
}