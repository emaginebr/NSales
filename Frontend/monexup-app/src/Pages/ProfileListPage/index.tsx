import { useContext, useEffect, useState } from "react";
import Col from "react-bootstrap/esm/Col";
import { useTranslation } from "react-i18next";
import Container from "react-bootstrap/esm/Container";
import Row from "react-bootstrap/esm/Row";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCalendar, faDollar, faEdit, faPlus, faSearch, faTrash } from '@fortawesome/free-solid-svg-icons';
import Table from "react-bootstrap/esm/Table";
import { Link, useNavigate } from "react-router-dom";
import Button from 'react-bootstrap/Button';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import Dropdown from 'react-bootstrap/Dropdown';
import ProfileContext from "../../Contexts/Profile/ProfileContext";
import NetworkContext from "../../Contexts/Network/NetworkContext";
import { MessageToastEnum } from "../../DTO/Enum/MessageToastEnum";
import MessageToast from "../../Components/MessageToast";
import ProductContext from "../../Contexts/Product/ProductContext";

export default function ProfileListPage() {

    const { t } = useTranslation();

    let navigate = useNavigate();

    const networkContext = useContext(NetworkContext);
    const profileContext = useContext(ProfileContext);

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
    const showDeleteMessage = (message: string) => {
        setDialog(MessageToastEnum.Confirmation);
        setMessageText(message);
        setShowMessage(true);
    };

    useEffect(() => {
        if (networkContext.userNetwork) {
            profileContext.listByNetwork(networkContext.userNetwork.networkId).then((ret) => {
                if (!ret.sucesso) {
                    throwError(ret.mensagemErro);
                }
            });
        }
    }, []);

    return (
        <>
            <MessageToast
                dialog={dialog}
                showMessage={showMessage}
                messageText={messageText}
                onClose={() => setShowMessage(false)}
                onYes={async () => {
                    if (profileContext.profile) {
                        let ret = await profileContext.delete(profileContext.profile.profileId);
                        if (ret.sucesso) {
                            showSuccessMessage(t("profileListPage.profileSuccessfullyDeleted"));
                            let retList = await profileContext.listByNetwork(networkContext.userNetwork.networkId);
                            if (!retList.sucesso) {
                                throwError(ret.mensagemErro);
                            }
                        }
                        else {
                            throwError(ret.mensagemErro);
                        }
                    }
                    setShowMessage(false);
                }}
                onNo={() => setShowMessage(false)}
            ></MessageToast>
            <Container>
                <Row>
                    <Col md="8">
                        <h3>
                            <nav aria-label="breadcrumb">
                                <ol className="breadcrumb">
                                    <li className="breadcrumb-item"><Link to="/admin/dashboard">{t("profileListPage.myNetwork")}</Link></li>
                                    <li className="breadcrumb-item active" aria-current="page">{t("profileListPage.networkTeamStructure")}</li>
                                </ol>
                            </nav>
                        </h3>
                    </Col>
                    <Col md="4" style={{ textAlign: "right" }}>
                        {/*
                        <InputGroup className="pull-right">
                            <Dropdown>
                                <Dropdown.Toggle variant="danger" id="dropdown-basic">
                                    Filter by: All Status
                                </Dropdown.Toggle>

                                <Dropdown.Menu>
                                    <Dropdown.Item href="#/action-1">Action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-2">Another action</Dropdown.Item>
                                    <Dropdown.Item href="#/action-3">Something else</Dropdown.Item>
                                </Dropdown.Menu>
                            </Dropdown>
                            <Button variant="primary" onClick={(e) => {
                                e.preventDefault();
                                navigate("/admin/team-structure/new");
                            }}><FontAwesomeIcon icon={faPlus} fixedWidth /> New</Button>
                        </InputGroup>
                        */}
                    </Col>
                </Row>
                <Row className="py-4">
                    <Col md="12">
                        <Table striped bordered hover>
                            <thead>
                                <tr>
                                    <th>{t("profileListPage.profileName")}</th>
                                    <th style={{ textAlign: "right" }}>{t("profileListPage.level")}</th>
                                    <th style={{ textAlign: "right" }}>{t("profileListPage.commission")} (%)</th>
                                    <th style={{ textAlign: "right" }}>{t("profileListPage.members")}</th>
                                    <th>{t("profileListPage.actions")}</th>
                                </tr>
                            </thead>
                            <tbody>
                                {
                                    profileContext.loading &&
                                    <tr>
                                        <td colSpan={5}>
                                            <div className="d-flex justify-content-center">
                                                <div className="spinner-border" role="status">
                                                    <span className="visually-hidden">{t("loading")}...</span>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                }
                                {!profileContext.loading && profileContext.profiles.map((profile) => {
                                    return (
                                        <tr>
                                            <td><Link to={"/admin/team-structure/" + profile.profileId}>{profile.name}</Link></td>
                                            <td style={{ textAlign: "right" }}>{profile.level}</td>
                                            <td style={{ textAlign: "right" }}>{profile.commission}%</td>
                                            <td style={{ textAlign: "right" }}>{profile.members}</td>
                                            <td>
                                                <Link to={"/admin/team-structure/" + profile.profileId}>
                                                    <FontAwesomeIcon icon={faEdit} fixedWidth />
                                                </Link>
                                                <a href="#" onClick={(e) => {
                                                    e.preventDefault();
                                                    showDeleteMessage(t("areYouSure"));
                                                }}><FontAwesomeIcon icon={faTrash} fixedWidth /></a>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </Table>
                    </Col>
                </Row>
            </Container>
        </>
    );
}