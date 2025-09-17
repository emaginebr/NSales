# 📋 Requisitos do Sistema MonexUp

## 1. Requisitos Gerais
1.1 O sistema deve permitir o funcionamento com múltiplas redes independentes.  
1.2 Cada rede deve possuir sua própria configuração de planos, representantes e doadores.  
1.3 O sistema deve suportar os perfis: Doador, Representante, Administrador de Rede e Administrador Master.

## 2. Tela de Doação
2.1 Cada representante deve ter um link único para compartilhamento da página de doação;  
2.2 Os planos devem ser exibidos conforme a configuração da rede (visíveis ou ocultos);
2.3 Tipos de planos: Doação única; Doações limitadas (ex: 12 parcelas); e Doações recorrentes.

2.4 Valores dos planos: fixos ou personalizáveis.  
2.5 Meios de pagamento: Cartão de crédito (padrão), PIX e Boleto.  
2.6 Cadastro de doadores: nome, e-mail, CPF (obrigatórios), telefone (opcional).  
2.7 Validação de e-mail: antes ou após a doação.  
2.8 Pagamentos pendentes devem ser processados automaticamente.

## 3. Cadastro de Representantes
3.1 Mesmo formulário do doador com campos adicionais: Dados bancários; e Envio de documentos;
3.2 Representante deve ser vinculado via link de indicação.  
3.3 Estados: Aguardando aprovação, Ativo, Bloqueado.  
3.4 Administrador da rede pode aprovar ou bloquear representantes.

## 4. Perfil Doador
4.1 Visualizar extrato de todas as doações.  
4.2 Pagamento de doações pendentes.  
4.3 Pertencer a múltiplas redes e alternar entre elas.

## 5. Perfil Representante
5.1 Visualizar extrato de comissões por rede.  
5.2 Acesso a links únicos de doação e indicação.  
5.3 Solicitação de saque com dados bancários/documentos válidos.  
5.4 Visualizar lista de indicados com nome, status, data e doações.  
5.5 Aprovar ou bloquear indicados.

## 6. Perfil Administrador de Rede
6.1 Cadastro e configuração da rede: Nome, e-mail, templates de e-mail; e Valor mínimo e periodicidade de saque;
6.2 Cadastro de níveis com percentuais por nível.  
6.3 Percentuais específicos por representante.  
6.4 Cadastro de planos: Nome; Valor; Frequência; e Visibilidade.
6.5 Listar e editar usuários da rede: status, permissões, nível, rede.  
6.6 Visualizar extrato geral da rede por representante.

## 7. Perfil Administrador Master
7.1 Acesso total a todas as redes.  
7.2 Cadastro de novas redes.  
7.3 Acesso a todos os recursos dos administradores de rede.  
7.4 Alternar entre redes pelo seletor no menu superior.
