import os
import sys
import re
import requests
from dotenv import load_dotenv
from openai import OpenAI

from seed_product_types import (
    seed_product_type_tree,
    link_category_to_product_type,
)

load_dotenv(override=True)

NAUTH_EMAIL = os.getenv("NAUTH_EMAIL")
NAUTH_PASSWORD = os.getenv("NAUTH_PASSWORD")
NAUTH_URL = os.getenv("NAUTH_URL")
LOFN_URL = os.getenv("LOFN_URL")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY")

TENANT_ID = "emagine"
DEVICE_FINGERPRINT = "seed-script"
USER_AGENT = "LofnSeedScript/1.0"

COMMON_HEADERS = {
    "X-Tenant-Id": TENANT_ID,
    "X-Device-Fingerprint": DEVICE_FINGERPRINT,
    "User-Agent": USER_AGENT,
}

PHOTOS_DIR = os.path.join(os.path.dirname(__file__), "photos", "info-store")
os.makedirs(PHOTOS_DIR, exist_ok=True)

ROOT_CATEGORY = "Informática"

PRODUCT_TYPES = {
    "Equipamento": {
        "name": "Equipamento",
        "filters": [
            {"label": "Marca", "data_type": "text"},
            {"label": "Modelo", "data_type": "text"},
            {"label": "Garantia_meses", "data_type": "integer"},
            {
                "label": "Sistema_Operacional",
                "data_type": "enum",
                "allowed_values": ["Windows", "macOS", "Linux", "ChromeOS", "Sem SO"],
            },
        ],
        "customization_groups": [
            {
                "label": "Processador",
                "selection_mode": "single",
                "is_required": True,
                "options": [
                    {"label": "Intel Core i3", "price_delta_cents": 0, "is_default": True},
                    {"label": "Intel Core i5", "price_delta_cents": 50000},
                    {"label": "Intel Core i7", "price_delta_cents": 90000},
                    {"label": "Intel Core i9", "price_delta_cents": 150000},
                ],
            },
            {
                "label": "Memória RAM",
                "selection_mode": "single",
                "is_required": True,
                "options": [
                    {"label": "8GB", "price_delta_cents": 0, "is_default": True},
                    {"label": "16GB", "price_delta_cents": 30000},
                    {"label": "32GB", "price_delta_cents": 70000},
                ],
            },
            {
                "label": "Armazenamento",
                "selection_mode": "single",
                "is_required": False,
                "options": [
                    {"label": "SSD 256GB", "price_delta_cents": 0, "is_default": True},
                    {"label": "SSD 512GB", "price_delta_cents": 25000},
                    {"label": "SSD 1TB", "price_delta_cents": 60000},
                ],
            },
        ],
    },
    "Componente PC": {
        "name": "Componente PC",
        "filters": [
            {"label": "Marca", "data_type": "text"},
            {"label": "Modelo", "data_type": "text"},
            {"label": "Capacidade", "data_type": "text"},
            {"label": "Garantia_meses", "data_type": "integer"},
        ],
    },
    "Periférico": {
        "name": "Periférico",
        "filters": [
            {"label": "Marca", "data_type": "text"},
            {"label": "Modelo", "data_type": "text"},
            {
                "label": "Conexão",
                "data_type": "enum",
                "allowed_values": ["USB", "USB-C", "Bluetooth", "Wireless 2.4GHz", "PS/2"],
            },
            {"label": "RGB", "data_type": "boolean"},
        ],
    },
}

# Mapeia subcategoria → nome do product type (chave em PRODUCT_TYPES).
CATEGORY_TYPE_LINKS = {
    "Notebook": "Equipamento",
    "Processadores": "Componente PC",
    "Memórias RAM": "Componente PC",
    "HDs": "Componente PC",
    "Gabinetes": "Componente PC",
    "Teclados": "Periférico",
    "Mouses": "Periférico",
}

CATEGORIES = {
    "Notebook": [
        {
            "name": "Notebook Gamer Liquidação",
            "price": 6499.00,
            "discount": 35,
            "featured": True,
            "description": (
                "## Notebook Gamer Liquidação\n\n"
                "**MEGA PROMOÇÃO**: notebook gamer com configuração robusta a "
                "**35% OFF**. Estoque limitado, modelo de mostruário em estado de novo.\n\n"
                "### Especificações\n\n"
                "| Spec | Detalhe |\n"
                "|------|---------|\n"
                "| Processador | AMD Ryzen 7 5800H |\n"
                "| Memória | 16GB DDR4 |\n"
                "| Armazenamento | 512GB NVMe SSD |\n"
                "| Vídeo | NVIDIA RTX 3060 6GB |\n"
                "| Tela | 15.6\" 144Hz IPS |\n\n"
                "> *Liquidação de mostruário — 35% OFF enquanto durar o estoque.*"
            ),
        },
        {
            "name": "MacBook Air M3",
            "price": 12999.00,
            "discount": 0,
            "featured": True,
            "description": (
                "## MacBook Air M3\n\n"
                "O **MacBook Air** com o revolucionário chip **Apple M3** entrega desempenho excepcional "
                "em um design ultrafino e silencioso, sem ventoinha.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Processador | Apple M3 (8-core CPU, 10-core GPU) |\n"
                "| Memória | 8GB RAM unificada |\n"
                "| Armazenamento | 256GB SSD |\n"
                "| Tela | 13.6\" Liquid Retina, 2560x1664 |\n"
                "| Bateria | Até 18 horas de uso |\n"
                "| Peso | 1.24 kg |\n\n"
                "### Destaques\n\n"
                "- **Neural Engine** de 16 núcleos para tarefas de IA e Machine Learning\n"
                "- Suporte a até **2 monitores externos**\n"
                "- **MagSafe** para carregamento seguro\n"
                "- Webcam **1080p FaceTime HD**\n"
                "- Áudio com **4 alto-falantes** e Spatial Audio\n\n"
                "> *Ideal para profissionais criativos, estudantes e quem busca portabilidade "
                "sem abrir mão de performance.*"
            ),
        },
        {
            "name": "Dell Inspiron 15",
            "price": 3499.00,
            "discount": 10,
            "description": (
                "## Dell Inspiron 15\n\n"
                "O **Dell Inspiron 15** é a escolha inteligente para quem precisa de um notebook "
                "versátil para trabalho e estudos, com ótimo custo-benefício.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Processador | Intel Core i5-1235U (12ª Geração) |\n"
                "| Memória | 8GB DDR4 3200MHz |\n"
                "| Armazenamento | 512GB SSD PCIe NVMe |\n"
                "| Tela | 15.6\" Full HD, antirreflexo |\n"
                "| Sistema | Windows 11 Home |\n"
                "| Peso | 1.65 kg |\n\n"
                "### Destaques\n\n"
                "- **ExpressCharge** — carregue 80% da bateria em 60 minutos\n"
                "- Teclado com **teclas confortáveis** de tamanho padrão\n"
                "- Conectividade **Wi-Fi 6** e Bluetooth 5.2\n"
                "- Slot para expansão de memória RAM\n"
                "- Design fino com bordas estreitas **ComfortView Plus** (low blue light)\n\n"
                "> *Perfeito para o dia a dia: navegação, Office, videochamadas e streaming.*"
            ),
        },
        {
            "name": "Lenovo IdeaPad 3",
            "price": 2899.00,
            "description": (
                "## Lenovo IdeaPad 3\n\n"
                "O **Lenovo IdeaPad 3** combina desempenho AMD com um preço acessível, "
                "sendo uma excelente opção de entrada para multitarefas.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Processador | AMD Ryzen 5 5500U |\n"
                "| Memória | 8GB DDR4 3200MHz |\n"
                "| Armazenamento | 256GB SSD PCIe NVMe |\n"
                "| Tela | 15.6\" Full HD, IPS, 300 nits |\n"
                "| Bateria | Até 10 horas |\n"
                "| Peso | 1.63 kg |\n\n"
                "### Destaques\n\n"
                "- **Dolby Audio** para som imersivo\n"
                "- Leitor de **impressão digital** integrado\n"
                "- **Rapid Charge** — 2 horas de uso com 15 min de carga\n"
                "- Webcam HD com **privacy shutter** físico\n"
                "- Chassi slim em **alumínio escovado**\n\n"
                "> *Ideal para estudantes e profissionais que buscam mobilidade com bom desempenho.*"
            ),
        },
        {
            "name": "ASUS VivoBook",
            "price": 3199.00,
            "description": (
                "## ASUS VivoBook 15\n\n"
                "O **ASUS VivoBook 15** é um notebook elegante e leve, projetado para "
                "produtividade e entretenimento com tela **NanoEdge**.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Processador | Intel Core i5-1235U (12ª Geração) |\n"
                "| Memória | 8GB DDR4 |\n"
                "| Armazenamento | 512GB SSD PCIe NVMe |\n"
                "| Tela | 15.6\" Full HD, IPS, NanoEdge |\n"
                "| Gráficos | Intel Iris Xe |\n"
                "| Peso | 1.7 kg |\n\n"
                "### Destaques\n\n"
                "- Tela com bordas ultrafinas **NanoEdge** (86% screen-to-body ratio)\n"
                "- **ASUS SonicMaster** para áudio de alta qualidade\n"
                "- **ErgoLift hinge** — inclina o teclado para digitação confortável\n"
                "- Leitor de impressão digital no touchpad\n"
                "- Disponível em **4 cores**: azul, prata, preto e verde\n\n"
                "> *Para quem quer estilo e funcionalidade em um só notebook.*"
            ),
        },
        {
            "name": "Acer Nitro 5",
            "price": 5499.00,
            "discount": 15,
            "featured": True,
            "description": (
                "## Acer Nitro 5\n\n"
                "O **Acer Nitro 5** é um notebook gamer de entrada que entrega performance "
                "real para jogos AAA e tarefas pesadas como edição de vídeo.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Processador | Intel Core i7-12700H (14 cores) |\n"
                "| Memória | 16GB DDR4 3200MHz |\n"
                "| Armazenamento | 512GB SSD PCIe NVMe |\n"
                "| GPU | NVIDIA GeForce GTX 1650 4GB GDDR6 |\n"
                "| Tela | 15.6\" Full HD IPS, **144Hz** |\n"
                "| Peso | 2.2 kg |\n\n"
                "### Destaques\n\n"
                "- Tela **144Hz** para gameplay fluido e sem tearing\n"
                "- Sistema de refrigeração **CoolBoost** com 2 ventoinhas e 4 exaustores\n"
                "- Teclado **retroiluminado RGB** de 4 zonas\n"
                "- **Killer Wi-Fi 6** para latência mínima em jogos online\n"
                "- Porta **Thunderbolt 4** e HDMI 2.1\n\n"
                "### Performance em Jogos\n\n"
                "| Jogo | FPS Médio (1080p) |\n"
                "|------|-------------------|\n"
                "| CS2 | ~120 fps |\n"
                "| Fortnite | ~90 fps |\n"
                "| Valorant | ~150 fps |\n\n"
                "> *O ponto de entrada perfeito para o mundo gamer com orçamento controlado.*"
            ),
        },
    ],
    "Processadores": [
        {
            "name": "AMD Ryzen 7 5800X",
            "price": 1599.00,
            "description": (
                "## AMD Ryzen 7 5800X\n\n"
                "O **Ryzen 7 5800X** é um processador de alto desempenho da arquitetura **Zen 3**, "
                "perfeito para gaming e workloads de criação de conteúdo.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Cores / Threads | 8 / 16 |\n"
                "| Clock Base | 3.8 GHz |\n"
                "| Clock Boost | 4.7 GHz |\n"
                "| Cache | 32MB L3 |\n"
                "| TDP | 105W |\n"
                "| Socket | AM4 |\n\n"
                "### Destaques\n\n"
                "- Arquitetura **Zen 3** com ganho de ~19% em IPC vs. Zen 2\n"
                "- **Game Cache** de 32MB para baixa latência\n"
                "- Compatível com placas-mãe **B550** e **X570**\n"
                "- Suporte a **PCIe 4.0**\n"
                "- Desbloqueado para **overclock**\n\n"
                "> *Excelente para quem busca o melhor single-thread performance na plataforma AM4.*"
            ),
        },
        {
            "name": "Intel Core i7-13700K",
            "price": 2299.00,
            "discount": 5,
            "featured": True,
            "description": (
                "## Intel Core i7-13700K\n\n"
                "O **i7-13700K** da 13ª geração (Raptor Lake) é uma potência híbrida "
                "com núcleos de Performance e Eficiência.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Cores | 16 (8P + 8E) |\n"
                "| Threads | 24 |\n"
                "| Clock Base | 3.4 GHz (P-cores) |\n"
                "| Clock Boost | 5.4 GHz |\n"
                "| Cache | 30MB Intel Smart Cache |\n"
                "| TDP | 125W (PBP) / 253W (MTP) |\n"
                "| Socket | LGA 1700 |\n\n"
                "### Destaques\n\n"
                "- Arquitetura híbrida **P-core + E-core** para multitarefa extrema\n"
                "- **Intel Thread Director** otimiza a distribuição de tarefas automaticamente\n"
                "- Suporte a **DDR5** e DDR4\n"
                "- Até **20 lanes PCIe 5.0**\n"
                "- Multiplicador **desbloqueado** para overclock\n\n"
                "> *Top de linha para gamers e criadores que exigem o máximo de desempenho.*"
            ),
        },
        {
            "name": "AMD Ryzen 5 5600X",
            "price": 899.00,
            "description": (
                "## AMD Ryzen 5 5600X\n\n"
                "O **Ryzen 5 5600X** é o processador mais popular da série Zen 3, "
                "oferecendo desempenho excepcional por um preço justo.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Cores / Threads | 6 / 12 |\n"
                "| Clock Base | 3.7 GHz |\n"
                "| Clock Boost | 4.6 GHz |\n"
                "| Cache | 32MB L3 |\n"
                "| TDP | 65W |\n"
                "| Socket | AM4 |\n\n"
                "### Destaques\n\n"
                "- **TDP de apenas 65W** — eficiente e fácil de refrigerar\n"
                "- Cooler **Wraith Stealth** incluso na caixa\n"
                "- Desempenho em jogos comparável a CPUs muito mais caras\n"
                "- Compatível com placas **B450**, B550 e X570\n"
                "- Excelente relação **custo-benefício**\n\n"
                "> *O queridinho dos gamers: 6 cores que dão conta de qualquer jogo moderno.*"
            ),
        },
        {
            "name": "Intel Core i5-13400F",
            "price": 1199.00,
            "description": (
                "## Intel Core i5-13400F\n\n"
                "O **i5-13400F** é um processador de excelente valor, com arquitetura "
                "híbrida e sem gráficos integrados — feito para quem já tem GPU dedicada.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Cores | 10 (6P + 4E) |\n"
                "| Threads | 16 |\n"
                "| Clock Base | 2.5 GHz (P-cores) |\n"
                "| Clock Boost | 4.6 GHz |\n"
                "| Cache | 20MB Intel Smart Cache |\n"
                "| TDP | 65W (PBP) / 148W (MTP) |\n"
                "| Socket | LGA 1700 |\n\n"
                "### Destaques\n\n"
                "- **10 cores** a um preço acessível\n"
                "- Sufixo **F** = sem iGPU (economia para quem tem placa de vídeo)\n"
                "- Suporte a memória **DDR5** e DDR4\n"
                "- Compatível com placas **B660**, B760 e Z790\n"
                "- Performance gaming acima do Ryzen 5 5600X em muitos títulos\n\n"
                "> *A melhor opção custo-benefício Intel para gaming em 2024.*"
            ),
        },
        {
            "name": "AMD Ryzen 9 7900X",
            "price": 3199.00,
            "description": (
                "## AMD Ryzen 9 7900X\n\n"
                "O **Ryzen 9 7900X** é um monstro de produtividade com a nova arquitetura "
                "**Zen 4** e suporte nativo a DDR5 e PCIe 5.0.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Cores / Threads | 12 / 24 |\n"
                "| Clock Base | 4.7 GHz |\n"
                "| Clock Boost | 5.6 GHz |\n"
                "| Cache | 64MB L3 |\n"
                "| TDP | 170W |\n"
                "| Socket | AM5 |\n\n"
                "### Destaques\n\n"
                "- **Zen 4** com até 13% mais IPC vs. Zen 3\n"
                "- Clock boost de **5.6 GHz** — o mais alto da linha\n"
                "- **64MB de cache L3** para workloads pesados\n"
                "- **RDNA 2 iGPU** integrada (saída de vídeo de emergência)\n"
                "- Plataforma **AM5** com suporte garantido até 2025+\n\n"
                "### Indicado Para\n\n"
                "- Renderização 3D e edição de vídeo 4K+\n"
                "- Compilação de código em projetos grandes\n"
                "- Streaming + gaming simultâneo\n"
                "- Virtualização e VMs\n\n"
                "> *Para profissionais e entusiastas que não aceitam compromissos.*"
            ),
        },
    ],
    "Memórias RAM": [
        {
            "name": "Corsair Vengeance 16GB DDR4",
            "price": 299.00,
            "description": (
                "## Corsair Vengeance LPX 16GB DDR4\n\n"
                "A **Corsair Vengeance LPX** é referência em confiabilidade e performance "
                "para DDR4, com perfil baixo ideal para coolers grandes.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 16GB (2x 8GB) |\n"
                "| Tipo | DDR4 |\n"
                "| Velocidade | 3200MHz |\n"
                "| Latência | CL16 |\n"
                "| Tensão | 1.35V |\n"
                "| Perfil XMP | 2.0 |\n\n"
                "### Destaques\n\n"
                "- **Perfil baixo (low-profile)** — compatível com coolers tower grandes\n"
                "- Dissipador de calor em **alumínio** para dissipação eficiente\n"
                "- **XMP 2.0** — basta ativar na BIOS para atingir 3200MHz\n"
                "- Testada com as principais placas Intel e AMD\n"
                "- Garantia **vitalícia** Corsair\n\n"
                "> *A escolha segura para qualquer build DDR4 — confiável e acessível.*"
            ),
        },
        {
            "name": "Kingston Fury 32GB DDR5",
            "price": 699.00,
            "description": (
                "## Kingston Fury Beast 32GB DDR5\n\n"
                "A **Kingston Fury Beast DDR5** leva sua build para a próxima geração "
                "com velocidades altíssimas e eficiência energética superior.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 32GB (2x 16GB) |\n"
                "| Tipo | DDR5 |\n"
                "| Velocidade | 5200MHz |\n"
                "| Latência | CL40 |\n"
                "| Tensão | 1.25V |\n"
                "| Perfil | Intel XMP 3.0 / AMD EXPO |\n\n"
                "### Destaques\n\n"
                "- **On-die ECC** — correção de erros embutida no chip\n"
                "- Suporte a **Intel XMP 3.0** e **AMD EXPO**\n"
                "- Dissipador de calor com design agressivo\n"
                "- **32GB** para multitarefa pesada e produtividade\n"
                "- Plug and play — reconhecida automaticamente pelo sistema\n\n"
                "> *O upgrade DDR5 que sua build nova geração merece.*"
            ),
        },
        {
            "name": "HyperX 8GB DDR4",
            "price": 159.00,
            "description": (
                "## HyperX Fury 8GB DDR4\n\n"
                "A **HyperX Fury 8GB** é a memória de entrada ideal para builds econômicas, "
                "oferecendo confiabilidade Kingston a um preço imbatível.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 8GB (1x 8GB) |\n"
                "| Tipo | DDR4 |\n"
                "| Velocidade | 2666MHz |\n"
                "| Latência | CL16 |\n"
                "| Tensão | 1.2V |\n\n"
                "### Destaques\n\n"
                "- **Overclock automático** — detecta a plataforma e ajusta a frequência\n"
                "- Dissipador elegante em preto fosco\n"
                "- **Baixo consumo** de energia (1.2V)\n"
                "- Compatível com Intel e AMD\n"
                "- Ideal para upgrade de notebooks e PCs básicos\n\n"
                "> *Quer sair de 4GB? Este é o primeiro passo — simples, barato e eficaz.*"
            ),
        },
        {
            "name": "Crucial 16GB DDR5",
            "price": 449.00,
            "description": (
                "## Crucial 16GB DDR5\n\n"
                "A **Crucial DDR5** oferece a tecnologia de próxima geração da **Micron** "
                "com ótimo custo-benefício para quem está migrando para DDR5.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 16GB (1x 16GB) |\n"
                "| Tipo | DDR5 |\n"
                "| Velocidade | 4800MHz |\n"
                "| Latência | CL40 |\n"
                "| Tensão | 1.1V |\n\n"
                "### Destaques\n\n"
                "- Fabricada pela **Micron** — chips de primeira linha\n"
                "- **On-die ECC** para integridade de dados\n"
                "- Voltagem de apenas **1.1V** — a mais eficiente da categoria\n"
                "- Fácil expansão — adicione outro pente para 32GB\n"
                "- Compatível com Intel 12ª/13ª/14ª geração e AMD AM5\n\n"
                "> *Entrada acessível no mundo DDR5 — confiável e eficiente.*"
            ),
        },
        {
            "name": "G.Skill Trident 32GB DDR4",
            "price": 599.00,
            "description": (
                "## G.Skill Trident Z RGB 32GB DDR4\n\n"
                "A **G.Skill Trident Z RGB** é a memória DDR4 premium para quem quer "
                "**performance máxima** e **visual espetacular** com RGB.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 32GB (2x 16GB) |\n"
                "| Tipo | DDR4 |\n"
                "| Velocidade | 3600MHz |\n"
                "| Latência | CL18 |\n"
                "| Tensão | 1.35V |\n"
                "| Perfil XMP | 2.0 |\n\n"
                "### Destaques\n\n"
                "- **Barra de LED RGB** no topo — compatível com todas as plataformas de sync\n"
                "- Sincroniza com **ASUS Aura**, MSI Mystic Light, Gigabyte RGB Fusion\n"
                "- **3600MHz CL18** — sweet spot para Ryzen\n"
                "- ICs Samsung B-Die selecionados\n"
                "- Design premium com acabamento **escovado em alumínio**\n\n"
                "> *A memória que une forma e função — para builds que precisam impressionar.*"
            ),
        },
    ],
    "HDs": [
        {
            "name": "Samsung 970 EVO 1TB NVMe",
            "price": 499.00,
            "discount": 20,
            "featured": True,
            "description": (
                "## Samsung 970 EVO Plus 1TB NVMe\n\n"
                "O **970 EVO Plus** é um SSD NVMe de referência no mercado, "
                "com velocidades extremas e a confiabilidade Samsung.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 1TB |\n"
                "| Interface | NVMe M.2 (PCIe 3.0 x4) |\n"
                "| Leitura Seq. | 3.500 MB/s |\n"
                "| Gravação Seq. | 3.300 MB/s |\n"
                "| NAND | Samsung V-NAND 3-bit MLC |\n"
                "| Durabilidade | 600 TBW |\n\n"
                "### Destaques\n\n"
                "- **Controlador Phoenix** otimizado pela Samsung\n"
                "- Tecnologia **Intelligent TurboWrite** para gravações rápidas\n"
                "- Software **Samsung Magician** para monitoramento e otimização\n"
                "- Encriptação **AES 256-bit** por hardware\n"
                "- Garantia de **5 anos**\n\n"
                "> *O padrão-ouro em SSDs NVMe — rápido, confiável e duradouro.*"
            ),
        },
        {
            "name": "WD Blue 2TB HDD",
            "price": 399.00,
            "description": (
                "## Western Digital Blue 2TB HDD\n\n"
                "O **WD Blue** é o HD mecânico mais confiável do mercado — "
                "ideal para armazenamento em massa de arquivos, jogos e mídia.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 2TB |\n"
                "| Interface | SATA III (6 Gb/s) |\n"
                "| Rotação | 7.200 RPM |\n"
                "| Cache | 256MB |\n"
                "| Form Factor | 3.5\" |\n\n"
                "### Destaques\n\n"
                "- **7200 RPM** — o mais rápido da linha Blue\n"
                "- **256MB de cache** para acesso rápido a arquivos frequentes\n"
                "- Tecnologia **NoTouch** — cabeça de leitura nunca toca o disco\n"
                "- Consumo baixo e operação **silenciosa**\n"
                "- Garantia de **2 anos** WD\n\n"
                "> *Para quem precisa de muito espaço sem gastar muito — o HD que não decepciona.*"
            ),
        },
        {
            "name": "Kingston A400 480GB SSD",
            "price": 199.00,
            "discount": 10,
            "description": (
                "## Kingston A400 480GB SSD\n\n"
                "O **Kingston A400** é o upgrade mais impactante que você pode fazer "
                "no seu PC — troque o HD por este SSD e sinta a diferença imediata.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 480GB |\n"
                "| Interface | SATA III (6 Gb/s) |\n"
                "| Leitura Seq. | 500 MB/s |\n"
                "| Gravação Seq. | 450 MB/s |\n"
                "| Form Factor | 2.5\" |\n"
                "| NAND | 3D TLC |\n\n"
                "### Destaques\n\n"
                "- Até **10x mais rápido** que um HD 7200RPM\n"
                "- Boot do Windows em **menos de 15 segundos**\n"
                "- **Sem partes móveis** — resistente a impactos e vibrações\n"
                "- Compatível com qualquer PC/notebook com SATA\n"
                "- Consumo de energia **ultra baixo**\n\n"
                "> *O upgrade mais barato que transforma seu PC velho em uma máquina nova.*"
            ),
        },
        {
            "name": "Seagate Barracuda 4TB",
            "price": 599.00,
            "description": (
                "## Seagate Barracuda 4TB\n\n"
                "O **Seagate Barracuda 4TB** é a solução para quem precisa de "
                "**armazenamento massivo** para coleções de jogos, vídeos e backups.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 4TB |\n"
                "| Interface | SATA III (6 Gb/s) |\n"
                "| Rotação | 5.400 RPM |\n"
                "| Cache | 256MB |\n"
                "| Form Factor | 3.5\" |\n\n"
                "### Destaques\n\n"
                "- **4TB** de espaço — cabem ~80 jogos AAA ou ~1000 horas de vídeo HD\n"
                "- Tecnologia **Multi-Tier Caching** (MTC) para performance otimizada\n"
                "- Operação a **5400 RPM** — silencioso e econômico\n"
                "- Ideal como **drive secundário** junto com um SSD de boot\n"
                "- Garantia de **2 anos** Seagate\n\n"
                "> *Espaço não será mais problema — guarde tudo sem preocupação.*"
            ),
        },
        {
            "name": "Samsung 870 EVO 500GB SSD",
            "price": 299.00,
            "description": (
                "## Samsung 870 EVO 500GB SSD\n\n"
                "O **870 EVO** é o SSD SATA mais rápido do mundo, "
                "combinando velocidade máxima com a lendária confiabilidade Samsung.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Capacidade | 500GB |\n"
                "| Interface | SATA III (6 Gb/s) |\n"
                "| Leitura Seq. | 560 MB/s |\n"
                "| Gravação Seq. | 530 MB/s |\n"
                "| NAND | Samsung V-NAND 3-bit MLC |\n"
                "| Durabilidade | 300 TBW |\n\n"
                "### Destaques\n\n"
                "- **Velocidade máxima** da interface SATA (560 MB/s)\n"
                "- Controlador **Samsung MKX** para performance consistente\n"
                "- **Intelligent TurboWrite** — buffer dinâmico para gravações sustentadas\n"
                "- Software **Samsung Magician** incluso\n"
                "- Garantia de **5 anos** ou 300 TBW\n\n"
                "> *Se seu PC só tem SATA, este é o melhor SSD que existe — ponto final.*"
            ),
        },
    ],
    "Gabinetes": [
        {
            "name": "NZXT H510",
            "price": 499.00,
            "description": (
                "## NZXT H510\n\n"
                "O **NZXT H510** é um gabinete mid-tower minimalista e elegante, "
                "referência em builds limpas e organizadas.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Tipo | Mid-Tower ATX |\n"
                "| Placas Suportadas | ATX, Micro-ATX, Mini-ITX |\n"
                "| Baias 3.5\" | 2 |\n"
                "| Baias 2.5\" | 2+1 |\n"
                "| Fans Inclusos | 2x 120mm (traseira + topo) |\n"
                "| GPU Máxima | 381mm |\n"
                "| Cooler CPU Máximo | 165mm |\n\n"
                "### Destaques\n\n"
                "- **Painel lateral em vidro temperado** com montagem em parafuso único\n"
                "- **Cable management** excepcional com canal traseiro e velcros\n"
                "- Painel frontal com **porta USB-C**\n"
                "- Filtro de poeira removível na parte inferior\n"
                "- Design **minimalista** com linhas limpas\n\n"
                "> *O gabinete que prova que menos é mais — build limpa, visual impecável.*"
            ),
        },
        {
            "name": "Corsair 4000D",
            "price": 599.00,
            "discount": 10,
            "featured": True,
            "description": (
                "## Corsair 4000D Airflow\n\n"
                "O **Corsair 4000D Airflow** é um dos melhores gabinetes para refrigeração, "
                "com painel frontal perfurado e amplo espaço interno.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Tipo | Mid-Tower ATX |\n"
                "| Placas Suportadas | ATX, Micro-ATX, Mini-ITX |\n"
                "| Fans Inclusos | 2x 120mm |\n"
                "| Suporte Radiador | Frontal 360mm, Topo 280mm |\n"
                "| GPU Máxima | 360mm |\n"
                "| Cooler CPU Máximo | 170mm |\n\n"
                "### Destaques\n\n"
                "- Painel frontal **high-airflow** com malha de aço perfurada\n"
                "- **Vidro temperado** lateral com dobradiça (abre sem ferramentas)\n"
                "- Suporte a **radiadores de até 360mm** na frente\n"
                "- Sistema **RapidRoute** para cable management limpo\n"
                "- Filtros de poeira **removíveis** em todas as entradas\n\n"
                "> *Airflow de sobra, espaço de sobra, qualidade de sobra — difícil errar com este.*"
            ),
        },
        {
            "name": "Cooler Master Q300L",
            "price": 299.00,
            "description": (
                "## Cooler Master MasterBox Q300L\n\n"
                "O **Q300L** é um gabinete Micro-ATX compacto e versátil, "
                "com painel magnético de poeira e design modular.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Tipo | Mini-Tower (Micro-ATX) |\n"
                "| Placas Suportadas | Micro-ATX, Mini-ITX |\n"
                "| Fans Inclusos | 1x 120mm (traseira) |\n"
                "| GPU Máxima | 360mm |\n"
                "| Cooler CPU Máximo | 159mm |\n"
                "| Peso | 3.88 kg |\n\n"
                "### Destaques\n\n"
                "- **Painel lateral em acrílico** transparente\n"
                "- Filtros de poeira **magnéticos** removíveis\n"
                "- I/O frontal com **USB 3.0**\n"
                "- Pode ser posicionado **horizontal ou vertical**\n"
                "- Surpreendentemente espaçoso — cabe **GPU de até 360mm**\n\n"
                "> *Compacto por fora, espaçoso por dentro — ideal para builds mATX econômicas.*"
            ),
        },
        {
            "name": "Lian Li O11",
            "price": 899.00,
            "description": (
                "## Lian Li O11 Dynamic\n\n"
                "O **Lian Li O11 Dynamic** é o gabinete premium escolhido por entusiastas "
                "e modders ao redor do mundo — um ícone de design.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Tipo | Mid-Tower ATX |\n"
                "| Placas Suportadas | E-ATX, ATX, Micro-ATX, Mini-ITX |\n"
                "| Fans Suportados | Até 9x 120mm |\n"
                "| Suporte Radiador | 360mm topo, frente e lateral |\n"
                "| GPU Máxima | 420mm |\n"
                "| Material | Alumínio + Vidro Temperado |\n\n"
                "### Destaques\n\n"
                "- **Duplo vidro temperado** — lateral e frontal\n"
                "- Suporte a até **3 radiadores de 360mm** simultaneamente\n"
                "- Câmara **dual-chamber** — PSU e cabos ficam escondidos\n"
                "- Construção em **alumínio** premium\n"
                "- Compatível com placas **E-ATX** (até 272mm)\n\n"
                "### Ideal Para\n\n"
                "- Custom water cooling loops\n"
                "- Builds showcase com muito RGB\n"
                "- Configurações multi-GPU\n\n"
                "> *O gabinete dos sonhos para quem quer uma build de vitrine.*"
            ),
        },
        {
            "name": "Redragon Wheel Jack",
            "price": 249.00,
            "description": (
                "## Redragon Wheel Jack\n\n"
                "O **Redragon Wheel Jack** entrega visual gamer com RGB incluso "
                "por um preço que cabe no bolso — ótimo custo-benefício.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Tipo | Mid-Tower ATX |\n"
                "| Placas Suportadas | ATX, Micro-ATX, Mini-ITX |\n"
                "| Fans Inclusos | 3x 120mm RGB |\n"
                "| GPU Máxima | 350mm |\n"
                "| Cooler CPU Máximo | 160mm |\n"
                "| Painel Lateral | Vidro Temperado |\n\n"
                "### Destaques\n\n"
                "- **3 fans RGB inclusos** — visual gamer pronto para uso\n"
                "- Painel lateral em **vidro temperado** (não acrílico)\n"
                "- Painel frontal com **mesh** para boa ventilação\n"
                "- Suporte a **radiador de 240mm** no topo\n"
                "- I/O frontal com **USB 3.0** e áudio HD\n\n"
                "> *Visual de gabinete premium, preço de entrada — impossível reclamar.*"
            ),
        },
    ],
    "Teclados": [
        {
            "name": "Logitech G Pro X",
            "price": 699.00,
            "featured": True,
            "description": (
                "## Logitech G Pro X\n\n"
                "O **G Pro X** é o teclado mecânico usado por jogadores profissionais de e-sports, "
                "com switches **hot-swappable** e construção premium.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Switches | GX Blue Clicky (hot-swap) |\n"
                "| Layout | TKL (sem numpad) |\n"
                "| Iluminação | RGB LIGHTSYNC por tecla |\n"
                "| Conexão | USB-C destacável |\n"
                "| Macros | Sim (Logitech G HUB) |\n"
                "| Construção | Corpo em aço |\n\n"
                "### Destaques\n\n"
                "- **Hot-swappable** — troque switches sem soldar (GX Blue, Red ou Brown)\n"
                "- Formato **TKL** compacto — mais espaço para o mouse\n"
                "- Cabo **USB-C destacável** para fácil transporte\n"
                "- **LIGHTSYNC RGB** com sync por jogo via G HUB\n"
                "- Usado por times como **G2, TSM e Cloud9**\n\n"
                "> *O teclado dos pros — compacto, customizável e construído para vencer.*"
            ),
        },
        {
            "name": "Razer BlackWidow V4",
            "price": 899.00,
            "description": (
                "## Razer BlackWidow V4\n\n"
                "O **BlackWidow V4** é o teclado mecânico flagship da Razer, "
                "com switches Green clicky e o melhor RGB do mercado — **Razer Chroma**.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Switches | Razer Green (clicky, 50g) |\n"
                "| Layout | Full-size com macro keys |\n"
                "| Iluminação | Razer Chroma RGB por tecla |\n"
                "| Wrist Rest | Magnético, acolchoado |\n"
                "| Media Controls | Dial + botões dedicados |\n"
                "| Durabilidade | 100 milhões de toques |\n\n"
                "### Destaques\n\n"
                "- **Razer Chroma** com 16.8M de cores e integração com 150+ jogos\n"
                "- **Macro keys** laterais programáveis\n"
                "- **Dial multifunção** para controle de mídia e volume\n"
                "- Apoio de pulso **magnético** removível e confortável\n"
                "- **Doubleshot PBT** keycaps — não desgastam com o tempo\n\n"
                "> *O teclado definitivo da Razer — tudo que um gamer pode querer em um só produto.*"
            ),
        },
        {
            "name": "HyperX Alloy Origins",
            "price": 549.00,
            "description": (
                "## HyperX Alloy Origins\n\n"
                "O **Alloy Origins** é um teclado mecânico compacto full-size com "
                "corpo de alumínio aircraft-grade e switches HyperX Red.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Switches | HyperX Red (linear, 45g) |\n"
                "| Layout | Full-size |\n"
                "| Iluminação | RGB por tecla |\n"
                "| Conexão | USB-C destacável |\n"
                "| Construção | Alumínio aircraft-grade |\n"
                "| Durabilidade | 80 milhões de toques |\n\n"
                "### Destaques\n\n"
                "- Corpo em **alumínio aeronáutico** — sólido e premium\n"
                "- Switches **HyperX Red lineares** — suaves e silenciosos\n"
                "- **RGB** com 3 níveis de inclinação ajustável\n"
                "- Cabo **USB-C removível** para portabilidade\n"
                "- **Memória onboard** para 3 perfis — funciona sem software\n\n"
                "> *Construção militar, toque suave, visual premium — o equilíbrio perfeito.*"
            ),
        },
        {
            "name": "Redragon Kumara K552",
            "price": 199.00,
            "discount": 25,
            "description": (
                "## Redragon Kumara K552\n\n"
                "O **Kumara K552** é o teclado mecânico mais vendido do Brasil — "
                "prova que mecânico bom não precisa ser caro.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Switches | Outemu Blue (clicky, 60g) |\n"
                "| Layout | TKL (sem numpad) |\n"
                "| Iluminação | LED vermelho (backlight) |\n"
                "| Conexão | USB com cabo trançado |\n"
                "| Construção | Base em metal |\n"
                "| Anti-Ghosting | N-Key Rollover |\n\n"
                "### Destaques\n\n"
                "- **Switches Outemu Blue** — feedback tátil e sonoro satisfatório\n"
                "- **Base em metal** — estabilidade surpreendente para o preço\n"
                "- **N-Key Rollover** — todas as teclas registradas simultaneamente\n"
                "- LED vermelho com **vários modos** de iluminação\n"
                "- Keycaps removíveis para limpeza e customização\n\n"
                "> *O rei do custo-benefício — seu primeiro mecânico ou backup confiável.*"
            ),
        },
        {
            "name": "Corsair K70 RGB",
            "price": 799.00,
            "description": (
                "## Corsair K70 RGB MK.2\n\n"
                "O **Corsair K70 RGB** é um clássico entre os teclados mecânicos — "
                "construção em alumínio escovado, Cherry MX e RGB deslumbrante.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Switches | Cherry MX Red (linear, 45g) |\n"
                "| Layout | Full-size com media keys |\n"
                "| Iluminação | RGB por tecla (iCUE) |\n"
                "| Wrist Rest | Magnético, texturizado |\n"
                "| Construção | Alumínio escovado |\n"
                "| USB Passthrough | Sim |\n\n"
                "### Destaques\n\n"
                "- **Cherry MX Red** — os switches mais confiáveis do mercado\n"
                "- Chassi em **alumínio escovado** anodizado\n"
                "- **iCUE RGB** com efeitos dinâmicos e sync com outros periféricos Corsair\n"
                "- **USB passthrough** para mouse ou headset\n"
                "- Apoio de pulso **magnético** texturizado incluso\n"
                "- **Tecla de torneio** — desativa Windows Key instantaneamente\n\n"
                "> *Um clássico que não envelhece — qualidade Cherry MX com o ecossistema Corsair.*"
            ),
        },
    ],
    "Mouses": [
        {
            "name": "Combo Setup Gamer (Mouse + Mousepad XL)",
            "price": 199.00,
            "discount": 28,
            "featured": True,
            "description": (
                "## Combo Setup Gamer\n\n"
                "**28% OFF**: combo com mouse gamer + mousepad XL com bordas costuradas. "
                "Promoção válida enquanto durarem os estoques.\n\n"
                "### Inclui\n\n"
                "- 1x Mouse gamer 7200 DPI\n"
                "- 1x Mousepad XL 90x40cm com bordas costuradas\n\n"
                "> *Pacote completo para upgrade rápido do setup, com 28% de desconto.*"
            ),
        },
        {
            "name": "Logitech G502 Hero",
            "price": 299.00,
            "discount": 15,
            "featured": True,
            "description": (
                "## Logitech G502 Hero\n\n"
                "O **G502 Hero** é o mouse gamer mais popular do mundo — "
                "sensor **HERO 25K**, 11 botões programáveis e pesos ajustáveis.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Sensor | HERO 25K (25.600 DPI) |\n"
                "| Botões | 11 programáveis |\n"
                "| Peso | 121g (+ até 16g em pesos) |\n"
                "| Taxa de Polling | 1000Hz |\n"
                "| Iluminação | RGB LIGHTSYNC |\n"
                "| Conexão | USB com cabo trançado |\n\n"
                "### Destaques\n\n"
                "- Sensor **HERO 25K** — rastreamento sub-pixel sem aceleração\n"
                "- **Sistema de pesos** — ajuste de 121g até 137g com 5 pesos de 3.6g\n"
                "- **Scroll infinito** ou ratchet — dual-mode com um clique\n"
                "- 11 botões programáveis via **G HUB**\n"
                "- **5 perfis de DPI** on-the-fly\n"
                "- Compatível com **POWERPLAY** (carregamento wireless)\n\n"
                "> *O mouse que agrada a todos — FPS, MOBA, RPG ou produtividade.*"
            ),
        },
        {
            "name": "Razer DeathAdder V3",
            "price": 499.00,
            "description": (
                "## Razer DeathAdder V3\n\n"
                "O **DeathAdder V3** é a evolução do mouse ergonômico mais vendido da história, "
                "agora com sensor **Focus Pro 30K** e peso de apenas 59g.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Sensor | Razer Focus Pro (30.000 DPI) |\n"
                "| Botões | 5 |\n"
                "| Peso | 59g |\n"
                "| Taxa de Polling | 8000Hz (HyperPolling) |\n"
                "| Switches | Razer Gen-3 Optical |\n"
                "| Conexão | USB Speedflex |\n\n"
                "### Destaques\n\n"
                "- **59g** — o DeathAdder mais leve de todos os tempos\n"
                "- Sensor **Focus Pro 30K** com Smart Tracking e Motion Sync\n"
                "- Switches **ópticos Gen-3** — 0.2ms de atuação, 90M cliques\n"
                "- **HyperPolling** a 8000Hz — 8x mais responsivo que 1000Hz\n"
                "- Formato ergonômico icônico refinado por 18 anos de feedback\n\n"
                "> *A lenda evoluiu — mais leve, mais rápido, mais preciso que nunca.*"
            ),
        },
        {
            "name": "HyperX Pulsefire Haste",
            "price": 249.00,
            "description": (
                "## HyperX Pulsefire Haste\n\n"
                "O **Pulsefire Haste** é um mouse ultra leve de apenas **59g** "
                "com design honeycomb e sensor PixArt de alta precisão.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Sensor | PixArt PAW3335 (16.000 DPI) |\n"
                "| Botões | 6 |\n"
                "| Peso | 59g |\n"
                "| Taxa de Polling | 1000Hz |\n"
                "| Switches | TTC Golden Micro |\n"
                "| Cabo | HyperFlex USB-C |\n\n"
                "### Destaques\n\n"
                "- Design **honeycomb** (hexagonal) para mínimo peso\n"
                "- Apenas **59g** — movimentos rápidos sem fadiga\n"
                "- Skates de **PTFE virgem** para deslizamento suave\n"
                "- Cabo **HyperFlex** trançado ultraleve — parece wireless\n"
                "- **Grip tapes** inclusos na caixa para customização\n"
                "- Switches **TTC Golden** duráveis (60M cliques)\n\n"
                "> *Leve como uma pena, preciso como uma agulha — feito para FPS competitivo.*"
            ),
        },
        {
            "name": "Redragon Cobra M711",
            "price": 99.00,
            "discount": 30,
            "description": (
                "## Redragon Cobra M711\n\n"
                "O **Cobra M711** é o mouse gamer mais vendido do Brasil — "
                "prova que performance não precisa custar caro.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Sensor | Pixart PAW3325 (10.000 DPI) |\n"
                "| Botões | 7 programáveis |\n"
                "| Peso | 85g |\n"
                "| Taxa de Polling | 1000Hz |\n"
                "| Iluminação | RGB Chroma (16.8M cores) |\n"
                "| Cabo | Trançado 1.8m |\n\n"
                "### Destaques\n\n"
                "- **7 botões** programáveis via software Redragon\n"
                "- **5 perfis de DPI** ajustáveis (até 10.000 DPI)\n"
                "- Iluminação **RGB** com vários efeitos\n"
                "- Sensor **Pixart** — tracking preciso sem aceleração\n"
                "- Formato ergonômico confortável para longas sessões\n"
                "- Skates de **teflon** para deslizamento suave\n\n"
                "> *R$ 99 por um mouse com sensor Pixart, RGB e 7 botões? Não tem igual.*"
            ),
        },
        {
            "name": "SteelSeries Rival 3",
            "price": 199.00,
            "description": (
                "## SteelSeries Rival 3\n\n"
                "O **Rival 3** é um mouse leve e preciso da SteelSeries, "
                "com sensor TrueMove Core e iluminação **Prism RGB**.\n\n"
                "### Especificações Técnicas\n\n"
                "| Spec | Detalhe |\n"
                "|------|--------|\n"
                "| Sensor | TrueMove Core (8.500 DPI) |\n"
                "| Botões | 6 |\n"
                "| Peso | 77g |\n"
                "| Taxa de Polling | 1000Hz |\n"
                "| Iluminação | Prism RGB (3 zonas) |\n"
                "| Switches | 60M cliques |\n\n"
                "### Destaques\n\n"
                "- Sensor **TrueMove Core** — co-desenvolvido com PixArt exclusivamente para SteelSeries\n"
                "- Apenas **77g** — leve sem ser honeycomb\n"
                "- **3 zonas de RGB** independentes com Prism\n"
                "- Integração com **SteelSeries GG** e Engine para customização total\n"
                "- Formato simétrico ambidestro-friendly\n"
                "- **Durabilidade** de 60 milhões de cliques\n\n"
                "> *Qualidade SteelSeries a um preço acessível — sensor premium, peso leve, RGB bonito.*"
            ),
        },
    ],
}


def slugify(text):
    text = text.lower().strip()
    text = re.sub(r"[àáâãäå]", "a", text)
    text = re.sub(r"[èéêë]", "e", text)
    text = re.sub(r"[ìíîï]", "i", text)
    text = re.sub(r"[òóôõö]", "o", text)
    text = re.sub(r"[ùúûü]", "u", text)
    text = re.sub(r"[ç]", "c", text)
    text = re.sub(r"[^a-z0-9]+", "-", text)
    text = text.strip("-")
    return text


def login():
    print(">> Fazendo login...")
    resp = requests.post(
        f"{NAUTH_URL}/User/loginWithEmail",
        json={"email": NAUTH_EMAIL, "password": NAUTH_PASSWORD},
        headers=COMMON_HEADERS,
    )
    resp.raise_for_status()
    data = resp.json()
    token = data.get("token") or data.get("accessToken")
    if not token:
        print(f"   Erro: resposta inesperada do login: {data}")
        sys.exit(1)
    print(f"   Login OK")
    return token


def create_store(token):
    print(">> Criando store 'Loja de Informática'...")
    resp = requests.post(
        f"{LOFN_URL}/store/insert",
        json={"name": "Loja de Informática"},
        headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
    )
    resp.raise_for_status()
    data = resp.json()
    slug = data["slug"]
    print(f"   Store criada: slug={slug}, id={data['storeId']}")
    return slug


def get_or_create_global_category(token, name, parent_id=None):
    label = f"'{name}'" if parent_id is None else f"'{name}' (parent={parent_id})"
    print(f"   Resolvendo categoria global {label}...")
    auth_headers = {**COMMON_HEADERS, "Authorization": f"Bearer {token}"}

    list_resp = requests.get(
        f"{LOFN_URL}/category-global/list",
        headers=auth_headers,
    )
    list_resp.raise_for_status()
    for item in list_resp.json() or []:
        same_name = (item.get("name") or "").strip().lower() == name.strip().lower()
        same_parent = item.get("parentCategoryId") == parent_id
        if same_name and same_parent:
            print(f"     -> já existe: categoryId={item['categoryId']}")
            return item["categoryId"]

    payload = {"name": name}
    if parent_id is not None:
        payload["parentCategoryId"] = parent_id

    create_resp = requests.post(
        f"{LOFN_URL}/category-global/insert",
        json=payload,
        headers=auth_headers,
    )
    create_resp.raise_for_status()
    data = create_resp.json()
    print(f"     -> nova categoria global: categoryId={data['categoryId']}, slug={data.get('slug')}")
    return data["categoryId"]


def create_product(token, store_slug, category_id, product):
    print(f"   Criando produto '{product['name']}'...")
    resp = requests.post(
        f"{LOFN_URL}/product/{store_slug}/insert",
        json={
            "categoryId": category_id,
            "name": product["name"],
            "description": product["description"],
            "price": product["price"],
            "discount": product.get("discount", 0),
            "frequency": 0,
            "limit": 0,
            "status": 1,  # Active
            "featured": product.get("featured", False),
        },
        headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
    )
    if not resp.ok:
        print(f"     ERRO HTTP {resp.status_code} ao criar '{product['name']}': {resp.text[:600]}")
        resp.raise_for_status()
    data = resp.json()
    print(f"     -> productId={data['productId']}, slug={data['slug']}")
    return data["productId"], data["slug"]


def generate_image(product_name, slug):
    filepath = os.path.join(PHOTOS_DIR, f"{slug}.png")
    if os.path.exists(filepath):
        print(f"     Imagem já existe: {slug}.png (cache)")
        return filepath

    print(f"     Gerando imagem com DALL-E para '{product_name}'...")
    client = OpenAI(api_key=OPENAI_API_KEY)
    prompt = (
        f"Professional product photo of {product_name}, "
        "white background, e-commerce style, high quality, studio lighting"
    )
    response = client.images.generate(
        model="dall-e-2",
        prompt=prompt,
        size="256x256",
        n=1,
    )
    image_url = response.data[0].url
    img_data = requests.get(image_url).content
    with open(filepath, "wb") as f:
        f.write(img_data)
    print(f"     Imagem salva: {slug}.png")
    return filepath


def upload_image(token, product_id, filepath):
    print(f"     Fazendo upload da imagem para productId={product_id}...")
    with open(filepath, "rb") as f:
        resp = requests.post(
            f"{LOFN_URL}/image/upload/{product_id}",
            params={"sortOrder": 0},
            files={"file": (os.path.basename(filepath), f, "image/png")},
            headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
        )
    resp.raise_for_status()
    data = resp.json()
    print(f"     Upload OK: imageId={data['imageId']}")


def main():
    if not all([NAUTH_EMAIL, NAUTH_PASSWORD, NAUTH_URL, LOFN_URL, OPENAI_API_KEY]):
        print("Erro: configure todas as variáveis no arquivo .env")
        sys.exit(1)

    masked_key = f"{OPENAI_API_KEY[:3]}...{OPENAI_API_KEY[-4:]}"
    print(f">> OpenAI API Key: {masked_key}")

    # Fase 1: Gerar todas as imagens via DALL-E
    print("\n========== FASE 1: Gerando imagens ==========\n")
    image_map = {}
    for category_name, products in CATEGORIES.items():
        print(f">> Categoria: {category_name}")
        for product in products:
            slug = slugify(product["name"])
            filepath = generate_image(product["name"], slug)
            image_map[product["name"]] = filepath

    # Fase 2: Criar dados na API do Lofn
    print("\n========== FASE 2: Criando dados na API ==========\n")
    token = login()
    store_slug = create_store(token)

    print(f"\n>> Categoria raiz: {ROOT_CATEGORY}")
    root_category_id = get_or_create_global_category(token, ROOT_CATEGORY)

    print("\n>> Criando product types e suas árvores (filtros + customizações)...")
    type_id_by_name = {}
    for type_key, spec in PRODUCT_TYPES.items():
        type_id_by_name[type_key] = seed_product_type_tree(
            token, COMMON_HEADERS, LOFN_URL, spec
        )

    for category_name, products in CATEGORIES.items():
        print(f"\n>> Subcategoria: {ROOT_CATEGORY}/{category_name}")
        category_id = get_or_create_global_category(token, category_name, parent_id=root_category_id)

        type_key = CATEGORY_TYPE_LINKS.get(category_name)
        if type_key and type_key in type_id_by_name:
            link_category_to_product_type(
                token, COMMON_HEADERS, LOFN_URL,
                category_id, type_id_by_name[type_key], marketplace=True,
            )

        for product in products:
            product_id, product_slug = create_product(
                token, store_slug, category_id, product
            )

            filepath = image_map[product["name"]]
            upload_image(token, product_id, filepath)

    print("\n>> Seed completo!")
    print(f"   Store: {store_slug}")
    print(f"   Categoria raiz: {ROOT_CATEGORY}")
    print(f"   Subcategorias: {len(CATEGORIES)}")
    print(f"   Produtos: {sum(len(p) for p in CATEGORIES.values())}")
    print(f"   Product types: {list(type_id_by_name.keys())}")


if __name__ == "__main__":
    main()
