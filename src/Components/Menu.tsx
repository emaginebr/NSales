import React, { useContext, useEffect, useState } from 'react';
import Container from 'react-bootstrap/Container';
import Button from 'react-bootstrap/esm/Button';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import { Link, useNavigate } from 'react-router-dom';
import AuthContext from '../Contexts/Auth/AuthContext';
import NavDropdown from 'react-bootstrap/NavDropdown';
import Alert from 'react-bootstrap/Alert';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faWarning } from '@fortawesome/free-solid-svg-icons/faWarning'
import { faBitcoinSign, faBoltLightning, faBox, faBrazilianRealSign, faBuilding, faCancel, faCheck, faCheckCircle, faCircle, faCircleUser, faClose, faCog, faCoins, faDollar, faEthernet, faFileWord, faHome, faImage, faLock, faPencil, faPhoneAlt, faPhotoVideo, faSearch, faSignInAlt, faUser, faUserAlt, faUserCircle, faUserCog, faUserFriends, faUserGear, faUserGraduate, faUserGroup, faUserMd } from '@fortawesome/free-solid-svg-icons';
import MessageToast from './MessageToast';
import { MessageToastEnum } from '../DTO/Enum/MessageToastEnum';
import { UserRoleEnum } from '../DTO/Enum/UserRoleEnum';
import NetworkContext from '../Contexts/Network/NetworkContext';
import InvoiceContext from '../Contexts/Invoice/InvoiceContext';
import StatementSearchParam from '../DTO/Domain/StatementSearchParam';
import { ImageModal, ImageTypeEnum } from './ImageModal';
import { useTranslation } from 'react-i18next';
import { MenuLanguage } from './Functions';


export default function Menu() {

  const [showAlert, setShowAlert] = useState<boolean>(true);
  const [showImageModal, setShowImageModal] = useState<boolean>(false);

  const [showMessage, setShowMessage] = useState<boolean>(false);
  const [messageText, setMessageText] = useState<string>("");

  const { t } = useTranslation();

  const throwError = (message: string) => {
    setMessageText(message);
    setShowMessage(true);
  };

  const showRoleText = (role: UserRoleEnum) => {
    switch (role) {
      case UserRoleEnum.NoRole:
        return (
          <span>
            <FontAwesomeIcon icon={faCancel} fixedWidth />&nbsp;{t("no_role")}
          </span>
        );
      case UserRoleEnum.User:
        return (
          <span>
            <FontAwesomeIcon icon={faUser} fixedWidth />&nbsp;{t("user")}
          </span>
        );
      case UserRoleEnum.Seller:
        return (
          <span>
            <FontAwesomeIcon icon={faUserMd} fixedWidth />&nbsp;{t("seller")}
          </span>
        );
      case UserRoleEnum.NetworkManager:
        return (
          <span>
            <FontAwesomeIcon icon={faUserGroup} fixedWidth />&nbsp;{t("network_manager")}
          </span>
        );
      case UserRoleEnum.Administrator:
        return (
          <span>
            <FontAwesomeIcon icon={faUserGear} fixedWidth />&nbsp;{t("administrator")}
          </span>
        );
      default:
        return null;
    }
  };

  let navigate = useNavigate();

  const authContext = useContext(AuthContext);
  const networkContext = useContext(NetworkContext);
  const invoiceContext = useContext(InvoiceContext);

  useEffect(() => {
    authContext.loadUserSession().then((authRet) => {
      if (authRet.sucesso) {
        networkContext.listByUser().then((ret) => {
          if (!ret.sucesso) {
            throwError(ret.mensagemErro);
          }
        });
      }
    });
  }, []);
  return (
    <>
      <MessageToast
        dialog={MessageToastEnum.Error}
        showMessage={showMessage}
        messageText={messageText}
        onClose={() => setShowMessage(false)}
      ></MessageToast>
      <ImageModal
        Image={ImageTypeEnum.User}
        show={showImageModal}
        onClose={() => setShowImageModal(false)}
      />
      <Navbar expand="lg" className="navbar-dark bg-dark mb-3 border-bottom">
        <Container>
          <Link className='navbar-brand' to="/">{process.env.REACT_APP_PROJECT_NAME}</Link>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto"> {/* Apply t() to link texts */}
              <Link className='nav-link' to="/"><FontAwesomeIcon icon={faHome} fixedWidth /> {t('home')}</Link>
              {!authContext.sessionInfo &&
                <Link className='nav-link' to="/new-seller"><FontAwesomeIcon icon={faUser} fixedWidth /> {t('be_a_representative')}</Link>
              }
              <Link className='nav-link' to="/network"><FontAwesomeIcon icon={faBuilding} fixedWidth /> {t('create_your_network')}</Link>
              {authContext.sessionInfo && networkContext.currentRole >= UserRoleEnum.Seller &&
                <NavDropdown title={
                  <>
                    <FontAwesomeIcon icon={faUserGroup} />&nbsp;{t('my_network')}
                  </>
                } id="basic-nav-dropdown">
                  {networkContext.currentRole == UserRoleEnum.NetworkManager &&
                    <>
                      <NavDropdown.ItemText className='small text-center'>{t('network_manager')}</NavDropdown.ItemText> {/* Or a more generic "Network" key */}
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/network");
                      }}><FontAwesomeIcon icon={faCog} fixedWidth />&nbsp;{t('preferences')}</NavDropdown.Item>
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/team-structure");
                      }}><FontAwesomeIcon icon={faUserCog} fixedWidth />&nbsp;{t('team_structure')}</NavDropdown.Item>
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/teams");
                      }}><FontAwesomeIcon icon={faUserGroup} fixedWidth />&nbsp;{t('teams')}</NavDropdown.Item>
                      <NavDropdown.Divider />
                    </>
                  }
                  {networkContext.currentRole >= UserRoleEnum.Seller &&
                    <>
                      <NavDropdown.ItemText className='small text-center'>{t('finances')}</NavDropdown.ItemText>
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/orders");
                      }}><FontAwesomeIcon icon={faFileWord} fixedWidth />&nbsp;{t('orders')}</NavDropdown.Item>
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/invoices");
                      }}><FontAwesomeIcon icon={faDollar} fixedWidth />&nbsp;{t('invoices')}</NavDropdown.Item>
                      <NavDropdown.Item onClick={() => {
                        navigate("/admin/products");
                      }}><FontAwesomeIcon icon={faBox} fixedWidth />&nbsp;{t('products')}</NavDropdown.Item>
                    </>
                  }
                </NavDropdown>
              }
            </Nav>
          </Navbar.Collapse>
          <Navbar.Collapse>

            <Nav className="ms-auto justify-content-end">
              {authContext.sessionInfo &&
                <>
                  {networkContext.userNetworks && networkContext.userNetworks.length > 0 &&
                    <NavDropdown title={
                      <>
                        {networkContext.userNetwork ?
                          <>
                            <FontAwesomeIcon icon={faUserGroup} />&nbsp;{networkContext.userNetwork?.network.name}
                          </>
                          :
                          <>
                            <FontAwesomeIcon icon={faCancel} />&nbsp;{t('no_network_selected')}
                          </>
                        }
                      </>
                    } id="basic-nav-dropdown"> {/* Use t() for texts */}
                      <NavDropdown.ItemText className='small'>{t('select_network_to_connect')}</NavDropdown.ItemText>
                      <NavDropdown.Divider />
                      {networkContext.userNetworks.map((network) => {
                        return (
                          <NavDropdown.Item onClick={() => {
                            networkContext.setUserNetwork(network);
                            navigate("/admin/dashboard");
                          }}><FontAwesomeIcon icon={faUserGroup} />&nbsp;{network.network.name}</NavDropdown.Item>
                        )
                      })}
                      <NavDropdown.Divider />
                      <NavDropdown.Item onClick={() => {
                        navigate("/network/search"); // This text also needs translation
                      }}><FontAwesomeIcon icon={faSearch} />&nbsp;{t('search_for_a_network')}</NavDropdown.Item>
                    </NavDropdown>
                  }
                  <NavDropdown title={showRoleText(networkContext.currentRole)} id="basic-nav-dropdown">
                    <NavDropdown.ItemText className='small'>{t('role_description')}</NavDropdown.ItemText>
                    <NavDropdown.Divider />
                    {networkContext.userNetwork?.role >= UserRoleEnum.User &&
                      <NavDropdown.Item onClick={(e) => {
                        e.preventDefault();
                        networkContext.setCurrentRole(UserRoleEnum.User);
                        navigate("/admin/dashboard");
                      }}>{showRoleText(UserRoleEnum.User)}</NavDropdown.Item>
                    }
                    {networkContext.userNetwork?.role >= UserRoleEnum.Seller &&
                      <NavDropdown.Item onClick={async (e) => {
                        e.preventDefault();
                        networkContext.setCurrentRole(UserRoleEnum.Seller);
                        var retBal = await invoiceContext.getBalance();
                        if (!retBal.sucesso) {
                          throwError(retBal.mensagemErro);
                        }
                        var retABal = await invoiceContext.getAvailableBalance();
                        if (!retABal.sucesso) {
                          throwError(retBal.mensagemErro);
                        }
                        let param: StatementSearchParam;
                        param = {
                          ...param,
                          userId: networkContext.userNetwork.userId,
                          pageNum: 1
                        };
                        var ret = await invoiceContext.searchStatement(param);
                        if (!ret.sucesso) {
                          throwError(ret.mensagemErro);
                        }
                        navigate("/admin/dashboard");
                      }}>{showRoleText(UserRoleEnum.Seller)}</NavDropdown.Item>
                    }
                    {networkContext.userNetwork?.role >= UserRoleEnum.NetworkManager &&
                      <NavDropdown.Item onClick={async (e) => {
                        e.preventDefault();
                        networkContext.setCurrentRole(UserRoleEnum.NetworkManager);
                        var retBal = await invoiceContext.getBalance(networkContext.userNetwork.networkId);
                        if (!retBal.sucesso) {
                          throwError(retBal.mensagemErro);
                        }

                        let param: StatementSearchParam;
                        param = {
                          ...param,
                          networkId: networkContext.userNetwork.networkId,
                          pageNum: 1
                        };
                        var ret = await invoiceContext.searchStatement(param);
                        if (!ret.sucesso) {
                          throwError(ret.mensagemErro);
                        }

                        navigate("/admin/dashboard");
                      }}>{showRoleText(UserRoleEnum.NetworkManager)}</NavDropdown.Item>
                    }
                    {networkContext.userNetwork?.role >= UserRoleEnum.Administrator &&
                      <NavDropdown.Item onClick={(e) => {
                        e.preventDefault();
                        networkContext.setCurrentRole(UserRoleEnum.Administrator);
                        navigate("/admin/dashboard");
                      }}>{showRoleText(UserRoleEnum.Administrator)}</NavDropdown.Item>
                    }
                  </NavDropdown>
                  {/*
                  <NavDropdown title={
                    <>
                      <FontAwesomeIcon icon={faCheckCircle} />&nbsp;Edição Ativada
                    </>
                  } id="basic-nav-dropdown">
                    <NavDropdown.ItemText className='small'>Ative o modo de edição para alterar as páginas da rede</NavDropdown.ItemText>
                    <NavDropdown.Divider />
                    <NavDropdown.Item><FontAwesomeIcon icon={faCheckCircle} />&nbsp;Ativar Edição</NavDropdown.Item>
                    <NavDropdown.Item><FontAwesomeIcon icon={faCircle} />&nbsp;Desativar Edição</NavDropdown.Item>
                  </NavDropdown>
                  */}
                </>
              }
              <MenuLanguage />
              {
                authContext.sessionInfo ?
                  <NavDropdown title={
                    <>
                      <FontAwesomeIcon icon={faCircleUser} />&nbsp;
                      <span>{authContext.sessionInfo.name}</span>
                    </>
                  } id="basic-nav-dropdown">
                    <NavDropdown.Item onClick={() => setShowImageModal(true)}>
                      <FontAwesomeIcon icon={faImage} fixedWidth /> {t('change_picture')}
                    </NavDropdown.Item>
                    <NavDropdown.Item onClick={async () => {
                      navigate("/account/edit-account");
                    }}><FontAwesomeIcon icon={faPencil} fixedWidth /> {t('edit_account')}</NavDropdown.Item>
                    <NavDropdown.Item onClick={async () => {
                      navigate("/account/change-password");
                    }}><FontAwesomeIcon icon={faLock} fixedWidth /> {t('change_password')}</NavDropdown.Item>
                    <NavDropdown.Divider />
                    <NavDropdown.Item onClick={async () => {
                      let ret = authContext.logout();
                      if (!ret.sucesso) {
                        throwError(ret.mensagemErro);
                      }
                      navigate(0);
                    }}><FontAwesomeIcon icon={faClose} fixedWidth /> {t('logout')}</NavDropdown.Item>
                  </NavDropdown>
                  :
                  <>
                    <Nav.Item>
                      <Button variant="danger" onClick={async () => {
                        navigate("/account/login");
                      }}> {/* Use t() for button text */}
                        <FontAwesomeIcon icon={faSignInAlt} fixedWidth /> {t('sign_in')}
                      </Button>
                    </Nav.Item>
                  </>
              }
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      {showAlert &&
        <Container>
          <Alert key="danger" variant="danger" onClose={() => setShowAlert(false)} dismissible>
            <FontAwesomeIcon icon={faWarning} /> {t('trial_version_warning')}
          </Alert>
        </Container>
      }
    </>
  );
}
