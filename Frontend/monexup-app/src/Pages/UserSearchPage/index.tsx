import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import { useTranslation } from "react-i18next";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faArrowDown, faArrowUp, faCalendar, faCancel, faCheck, faCheckCircle, faClose, faCross, faDollar, faEdit, faEnvelope, faPlus, faSearch, faTrash } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate, useParams } from "react-router-dom";
import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import Dropdown from 'react-bootstrap/Dropdown';
import Pagination from 'react-bootstrap/Pagination';
import MessageToast from "../../Components/MessageToast";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import UserContext from "../../Contexts/User/UserContext";
import { UserRoleEnum } from "../../DTO/Enum/UserRoleEnum";
import { UserNetworkStatusEnum } from "../../DTO/Enum/UserNetworkStatusEnum";

export default function UserSearchPage() {

    const { t } = useTranslation();

    let navigate = useNavigate();

    const userContext = useContext(UserContext);
    const networkContext = useContext(NetworkContext);

    let { pageNum } = useParams();

    const [dialog, setDialog] = useState<MessageToastEnum>(MessageToastEnum.Error);
    const [showMessage, setShowMessage] = useState<boolean>(false);
    const [messageText, setMessageText] = useState<string>("");

    const throwError = (message: string) => {
        setDialog(MessageToastEnum.Error);
        setMessageText(message);
        setShowMessage(true);
    };
    const showSuccessMessage = (message: string) => {
        setDialog(MessageToastEnum.Success);
        setMessageText(message);
        setShowMessage(true);
    };

    const showRole = (role: UserRoleEnum) => {
        let ret: string;
        switch (role) {
            case UserRoleEnum.NoRole:
                ret = t("userSearchPage.roles.noRole");
                break;
            case UserRoleEnum.User:
                ret = t("userSearchPage.roles.user");
                break;
            case UserRoleEnum.Seller:
                ret = t("userSearchPage.roles.seller");
                break;
            case UserRoleEnum.NetworkManager:
                ret = t("userSearchPage.roles.networkManager");
                break;
            case UserRoleEnum.Administrator:
                ret = t("userSearchPage.roles.administrator");
                break;
        }
        return ret;
    };

    const showStatus = (status: UserNetworkStatusEnum) => {
        let ret: string = "";
        switch (status) {
            case UserNetworkStatusEnum.Active:
                ret = t("userSearchPage.status.active");
                break;
            case UserNetworkStatusEnum.Blocked:
                ret = t("userSearchPage.status.blocked");
                break;
            case UserNetworkStatusEnum.Inactive:
                ret = t("userSearchPage.status.inactive");
                break;
            case UserNetworkStatusEnum.WaitForApproval:
                ret = t("userSearchPage.status.waitForApproval");
                break;
        }
        return ret;
    };

    const searchUsers = (pageNum: number) => {
        userContext.search(networkContext.userNetwork.networkId, "", pageNum, null).then((ret) => {
            if (!ret.sucesso) {
                throwError(ret.mensagemErro);
            }
        });
    };

    useEffect(() => {
        if (networkContext.userNetwork) {
            let pageNumInt: number = parseInt(pageNum);
            if (!pageNumInt) {
                pageNumInt = 1;
            }
            searchUsers(pageNumInt);
        }
    }, []);

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
            ></MessageToast>
            <Container>
                <Row>
                    <Col md="6">
                        <h3>
                            <nav aria-label="breadcrumb">
                                <ol className="breadcrumb">
                                    <li className="breadcrumb-item"><Link to="/admin/dashboard">{t("userSearchPage.breadcrumbs.myNetwork")}</Link></li>
                                    <li className="breadcrumb-item active" aria-current="page">{t("userSearchPage.breadcrumbs.networkTeam")}</li>
                                </ol>
                            </nav>
                        </h3>
                    </Col>
                    <Col md="6" style={{ textAlign: "right" }}>
                        <InputGroup className="pull-right">
                            <Form.Control
                                placeholder={t("userSearchPage.searchPlaceholder")}
                                aria-label={t("userSearchPage.searchPlaceholder")}
                            />
                            <Button variant="outline-secondary"><FontAwesomeIcon icon={faSearch} fixedWidth /></Button>
                            {/*}
                            <Dropdown>
                                <Dropdown.Toggle variant="danger" id="dropdown-basic">
                                    Filter by: All Profiles
                                </Dropdown.Toggle>

                                <Dropdown.Menu>
                                    <Dropdown.Item href="#/action-1">Action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-2">Another action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-3">Something else</Dropdown.Item>
                                </Dropdown.Menu>
                            </Dropdown>
                            */}
                            <Button variant="primary" disabled><FontAwesomeIcon icon={faEnvelope} fixedWidth /> {t("userSearchPage.inviteButton")}</Button>
                        </InputGroup>
                    </Col>
                </Row>
                <Row className="py-4">
                    <Col md="12">
                        <Table striped bordered hover>
                            <thead>
                                <tr>
                                    <th>{t("userSearchPage.tableHeaders.seller")}</th>
                                    <th>{t("userSearchPage.tableHeaders.profile")}</th>
                                    <th>{t("userSearchPage.tableHeaders.role")}</th>
                                    <th style={{ textAlign: "right" }}>{t("userSearchPage.tableHeaders.commission")} (%)</th>
                                    <th>{t("userSearchPage.tableHeaders.status")}</th>
                                    <th>{t("userSearchPage.tableHeaders.actions")}</th>
                                </tr>
                            </thead>
                            <tbody>
                                {
                                    userContext.loadingSearch &&
                                    <tr>
                                        <td colSpan={6}>
                                            <div className="d-flex justify-content-center">
                                                <div className="spinner-border" role="status">
                                                    <span className="visually-hidden">{t("loading")}...</span>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                }
                                {!userContext.loadingSearch && userContext.searchResult?.users.map((user) => {
                                    return (
                                        <tr>
                                            <td>{user.name}</td>
                                            <td>{user.profile}</td>
                                            <td>{showRole(user.role)}</td>
                                            <td style={{ textAlign: "right" }}>{user.commission}%</td>
                                            <td>{showStatus(user.status)}</td>
                                            <td>
                                                <>
                                                    <a href="#" className="text-success" onClick={async (e) => {
                                                        e.preventDefault();
                                                        let ret = await networkContext.promote(networkContext.network?.networkId, user.userId);
                                                        if (ret.sucesso) {
                                                            showSuccessMessage("User promoted");
                                                            searchUsers(userContext.searchResult?.pageNum);
                                                        }
                                                        else {
                                                            throwError(ret.mensagemErro);
                                                        }
                                                    }}>
                                                        <FontAwesomeIcon icon={faArrowUp} fixedWidth /> Promove
                                                    </a>
                                                    <a href="#" className="text-danger" onClick={async (e) => {
                                                        e.preventDefault();
                                                        let ret = await networkContext.demote(networkContext.network?.networkId, user.userId);
                                                        if (ret.sucesso) {
                                                            showSuccessMessage("User demoted");
                                                            searchUsers(userContext.searchResult?.pageNum);
                                                        }
                                                        else {
                                                            throwError(ret.mensagemErro);
                                                        }
                                                    }}>
                                                        <FontAwesomeIcon icon={faArrowDown} fixedWidth /> Demote
                                                    </a>
                                                </>
                                                {user.status == UserNetworkStatusEnum.Active &&
                                                    <>
                                                        <a href="#" className="text-danger" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Inactive);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userAccessRemoved"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faTrash} fixedWidth /> {t("userSearchPage.actions.remove")}
                                                        </a>
                                                    </>
                                                }
                                                {user.status == UserNetworkStatusEnum.Inactive &&
                                                    <>
                                                        <a href="#" className="text-danger" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Active);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userReactivated"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faCheck} fixedWidth /> {t("userSearchPage.actions.reactivate")}
                                                        </a>
                                                        <a href="#" className="text-danger" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Blocked);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userBlocked"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faCancel} fixedWidth /> {t("userSearchPage.actions.block")}
                                                        </a>
                                                    </>
                                                }
                                                {user.status == UserNetworkStatusEnum.WaitForApproval &&
                                                    <>
                                                        <a href="#" className="text-success" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Active);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userApproved"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faCheck} fixedWidth /> {t("userSearchPage.actions.approve")}
                                                        </a>
                                                        <a href="#" className="text-danger" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Inactive);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userReproved"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faClose} fixedWidth /> {t("userSearchPage.actions.reprove")}
                                                        </a>
                                                    </>
                                                }
                                                {user.status == UserNetworkStatusEnum.Blocked &&
                                                    <>
                                                        <a href="#" className="text-danger" onClick={async (e) => {
                                                            e.preventDefault();
                                                            let ret = await networkContext.changeStatus(networkContext.network?.networkId, user.userId, UserNetworkStatusEnum.Active);
                                                            if (ret.sucesso) {
                                                                showSuccessMessage(t("userSearchPage.messages.userReactivated"));
                                                                searchUsers(userContext.searchResult?.pageNum);
                                                            }
                                                            else {
                                                                throwError(ret.mensagemErro);
                                                            }
                                                        }}>
                                                            <FontAwesomeIcon icon={faCheck} fixedWidth /> {t("userSearchPage.actions.reactivate")}
                                                        </a>
                                                    </>
                                                }
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </Table>
                    </Col>
                </Row>
                {!userContext.loadingSearch && userContext.searchResult &&
                    <Row>
                        <Col md={12} className="text-center">
                            <Pagination className="justify-content-center">
                                <Pagination.First
                                    disabled={!(userContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchUsers(1)} />
                                <Pagination.Prev
                                    disabled={!(userContext.searchResult?.pageNum > 1)}
                                    onClick={() => searchUsers(userContext.searchResult?.pageNum - 1)} />
                                <Pagination.Ellipsis />

                                {userContext.searchResult?.pageNum - 2 > 1 &&
                                    <Pagination.Item
                                        onClick={() => searchUsers(userContext.searchResult?.pageNum - 2)}
                                    >{userContext.searchResult?.pageNum - 2}</Pagination.Item>
                                }
                                {userContext.searchResult?.pageNum - 1 > 1 &&
                                    <Pagination.Item
                                        onClick={() => searchUsers(userContext.searchResult?.pageNum - 1)}
                                    >{userContext.searchResult?.pageNum - 1}</Pagination.Item>
                                }
                                <Pagination.Item active>{userContext.searchResult?.pageNum}</Pagination.Item>
                                {userContext.searchResult?.pageNum + 1 <= userContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchUsers(userContext.searchResult?.pageNum + 1)}
                                    >{userContext.searchResult?.pageNum + 1}</Pagination.Item>
                                }
                                {userContext.searchResult?.pageNum + 2 <= userContext.searchResult?.pageCount &&
                                    <Pagination.Item
                                        onClick={() => searchUsers(userContext.searchResult?.pageNum + 2)}
                                    >{userContext.searchResult?.pageNum + 2}</Pagination.Item>
                                }

                                <Pagination.Ellipsis />
                                <Pagination.Next
                                    disabled={!(userContext.searchResult?.pageNum < userContext.searchResult?.pageCount)}
                                    onClick={() => searchUsers(userContext.searchResult?.pageCount)}
                                />
                                <Pagination.Last
                                    disabled={!(userContext.searchResult?.pageNum < userContext.searchResult?.pageCount)}
                                    onClick={() => searchUsers(userContext.searchResult?.pageCount)} />
                            </Pagination>
                        </Col>
                    </Row>
                }
            </Container>
        </>
    );
}