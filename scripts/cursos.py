import os
import sys
import re
import requests
from dotenv import load_dotenv
from openai import OpenAI

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

PHOTOS_DIR = os.path.join(os.path.dirname(__file__), "photos", "cursos")
os.makedirs(PHOTOS_DIR, exist_ok=True)

PRODUCT_TYPE_INFO = 2  # InfoProduct

ROOT_CATEGORY = "Cursos"

CATEGORIES = {
    "Programação": [
        {
            "name": "Full Stack com React e Node.js",
            "price": 497.00,
            "discount": 20,
            "featured": True,
            "description": (
                "## Full Stack com React e Node.js\n\n"
                "Curso completo de desenvolvimento **Full Stack** moderno — do zero ao deploy. "
                "Domine **React 18** no frontend e **Node.js** com Express no backend.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos Web | HTML5, CSS3, JavaScript ES6+ |\n"
                "| React 18 | Hooks, Context API, React Router, TanStack Query |\n"
                "| Node.js + Express | REST APIs, middleware, autenticação JWT |\n"
                "| Banco de Dados | PostgreSQL, Prisma ORM, migrations |\n"
                "| DevOps | Docker, CI/CD com GitHub Actions, deploy na AWS |\n"
                "| Projeto Final | E-commerce completo do zero ao deploy |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 120 horas de conteúdo |\n"
                "| Aulas | 380 vídeo-aulas |\n"
                "| Nível | Iniciante ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **Projeto real** de e-commerce construído ao longo do curso\n"
                "- Código-fonte completo no **GitHub** com cada etapa\n"
                "- Comunidade exclusiva no **Discord** com +5.000 alunos\n"
                "- Atualizações gratuitas sempre que houver nova versão do React/Node\n"
                "- Suporte direto com o instrutor via fórum\n\n"
                "> *Do primeiro console.log ao deploy em produção — tudo que você precisa para se tornar Full Stack.*"
            ),
        },
        {
            "name": "Python do Zero ao Profissional",
            "price": 297.00,
            "discount": 0,
            "featured": True,
            "description": (
                "## Python do Zero ao Profissional\n\n"
                "Aprenda **Python** da forma certa — começando pelos fundamentos e avançando até "
                "automação, web scraping, APIs e análise de dados.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Variáveis, estruturas de dados, funções, OOP |\n"
                "| Automação | Scripts, manipulação de arquivos, regex |\n"
                "| Web Scraping | BeautifulSoup, Selenium, Scrapy |\n"
                "| APIs | Flask, FastAPI, autenticação, documentação |\n"
                "| Dados | Pandas, NumPy, Matplotlib, Jupyter |\n"
                "| Projeto Final | Bot de automação + dashboard de dados |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 80 horas de conteúdo |\n"
                "| Aulas | 250 vídeo-aulas |\n"
                "| Nível | Iniciante ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **+200 exercícios** práticos com correção automática\n"
                "- Ambiente de código **online** — não precisa instalar nada para começar\n"
                "- Projetos reais: **web scraper**, **API REST**, **dashboard analítico**\n"
                "- Material atualizado para **Python 3.12**\n"
                "- Módulo bônus de **testes automatizados** com pytest\n\n"
                "> *A linguagem mais versátil do mercado — aprenda uma vez, use em tudo.*"
            ),
        },
        {
            "name": "C# e .NET para Desenvolvedores",
            "price": 397.00,
            "discount": 15,
            "featured": False,
            "description": (
                "## C# e .NET para Desenvolvedores\n\n"
                "Domine o ecossistema **Microsoft .NET** com C# — "
                "de aplicações console a APIs profissionais com **ASP.NET Core**.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| C# Fundamentos | Tipos, LINQ, async/await, generics, patterns |\n"
                "| ASP.NET Core | Minimal APIs, Controllers, middleware, DI |\n"
                "| Entity Framework | Code First, migrations, queries avançadas |\n"
                "| Arquitetura | Clean Architecture, CQRS, Repository Pattern |\n"
                "| Testes | xUnit, Moq, integration tests |\n"
                "| Deploy | Docker, Azure App Service, CI/CD |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 100 horas de conteúdo |\n"
                "| Aulas | 320 vídeo-aulas |\n"
                "| Nível | Intermediário ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- Foco em **Clean Architecture** — o padrão usado nas grandes empresas\n"
                "- Projeto completo de **API REST** com autenticação e autorização\n"
                "- Módulo de **Entity Framework Core 9** com PostgreSQL\n"
                "- Deploy automatizado com **Docker** e **GitHub Actions**\n"
                "- Bônus: introdução a **Blazor** para frontend com C#\n\n"
                "> *A stack enterprise que mais contrata no Brasil — domine .NET e destaque-se no mercado.*"
            ),
        },
    ],
    "IA": [
        {
            "name": "Machine Learning com Python",
            "price": 447.00,
            "discount": 10,
            "featured": True,
            "description": (
                "## Machine Learning com Python\n\n"
                "Curso prático de **Machine Learning** — dos conceitos matemáticos à implementação "
                "de modelos reais com **scikit-learn**, **TensorFlow** e **PyTorch**.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Estatística, álgebra linear, cálculo para ML |\n"
                "| Supervised Learning | Regressão, classificação, árvores, SVM |\n"
                "| Unsupervised Learning | Clustering, PCA, redução de dimensionalidade |\n"
                "| Deep Learning | Redes neurais, CNN, RNN, transformers |\n"
                "| NLP | Tokenização, embeddings, BERT, GPT fine-tuning |\n"
                "| MLOps | MLflow, model serving, monitoramento |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 90 horas de conteúdo |\n"
                "| Aulas | 280 vídeo-aulas |\n"
                "| Nível | Intermediário ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **15 projetos práticos** com datasets reais do Kaggle\n"
                "- Notebooks **Jupyter** prontos para cada aula\n"
                "- Módulo completo de **NLP** com transformers e fine-tuning de LLMs\n"
                "- Introdução a **MLOps** com MLflow e deploy de modelos\n"
                "- Acesso a **GPU gratuita** via Google Colab Pro durante o curso\n\n"
                "> *De dados brutos a modelos em produção — o caminho completo para se tornar ML Engineer.*"
            ),
        },
        {
            "name": "Prompt Engineering e ChatGPT",
            "price": 197.00,
            "discount": 25,
            "featured": False,
            "description": (
                "## Prompt Engineering e ChatGPT\n\n"
                "Domine a arte de criar **prompts eficazes** para ChatGPT, Claude e outros LLMs — "
                "e aprenda a integrar IA generativa em aplicações reais via **API**.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Como LLMs funcionam, tokens, contexto, temperature |\n"
                "| Técnicas de Prompt | Zero-shot, few-shot, chain-of-thought, ReAct |\n"
                "| APIs | OpenAI API, Anthropic API, streaming, function calling |\n"
                "| Aplicações | Chatbots, RAG, summarization, code generation |\n"
                "| Automação | LangChain, agentes autônomos, tools |\n"
                "| Ética | Viés, alucinações, segurança, guardrails |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 40 horas de conteúdo |\n"
                "| Aulas | 120 vídeo-aulas |\n"
                "| Nível | Iniciante ao Intermediário |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **Biblioteca de 200+ prompts** prontos para copiar e usar\n"
                "- Projetos com **OpenAI API** e **Anthropic API** (Claude)\n"
                "- Módulo de **RAG** (Retrieval-Augmented Generation) com embeddings\n"
                "- Construção de **agentes autônomos** com LangChain\n"
                "- Atualizado mensalmente com novos modelos e técnicas\n\n"
                "> *A habilidade mais valorizada de 2025 — quem sabe usar IA, multiplica sua produtividade.*"
            ),
        },
        {
            "name": "Computer Vision com OpenCV e YOLO",
            "price": 347.00,
            "discount": 0,
            "featured": False,
            "description": (
                "## Computer Vision com OpenCV e YOLO\n\n"
                "Aprenda **visão computacional** na prática — de processamento de imagens básico "
                "a detecção de objetos em tempo real com **YOLOv8** e **OpenCV**.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Pixels, filtros, transformações, histogramas |\n"
                "| OpenCV | Detecção de bordas, contornos, tracking, OCR |\n"
                "| Deep Learning | CNNs, transfer learning, data augmentation |\n"
                "| Detecção de Objetos | YOLO v5/v8, SSD, treinamento custom |\n"
                "| Segmentação | Semântica, instância, SAM (Segment Anything) |\n"
                "| Projeto Final | Sistema de monitoramento com câmera em tempo real |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 70 horas de conteúdo |\n"
                "| Aulas | 200 vídeo-aulas |\n"
                "| Nível | Intermediário ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **10 projetos práticos**: OCR, face detection, object tracking, pose estimation\n"
                "- Treinamento de modelo **YOLO custom** com seus próprios dados\n"
                "- Módulo de **SAM** (Segment Anything Model) da Meta\n"
                "- Deploy de modelos de CV com **FastAPI** e **ONNX Runtime**\n"
                "- Acesso a datasets curados para cada projeto\n\n"
                "> *Dê olhos à máquina — computer vision é a IA que transforma o mundo físico em dados.*"
            ),
        },
    ],
    "Banco de Dados": [
        {
            "name": "PostgreSQL Completo",
            "price": 247.00,
            "discount": 0,
            "featured": True,
            "description": (
                "## PostgreSQL Completo\n\n"
                "Domine o **PostgreSQL** do básico ao avançado — modelagem, queries complexas, "
                "performance tuning e administração de produção.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | DDL, DML, tipos de dados, constraints |\n"
                "| Queries Avançadas | JOINs, subqueries, CTEs, window functions |\n"
                "| Modelagem | Normalização, índices, particionamento |\n"
                "| Performance | EXPLAIN ANALYZE, índices compostos, vacuum |\n"
                "| Administração | Backup, replicação, pg_stat, monitoramento |\n"
                "| Extensões | PostGIS, pg_trgm, pgcrypto, full-text search |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 60 horas de conteúdo |\n"
                "| Aulas | 180 vídeo-aulas |\n"
                "| Nível | Iniciante ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **Ambiente Docker** pronto para praticar desde a primeira aula\n"
                "- **100+ exercícios** com datasets reais (e-commerce, financeiro, geográfico)\n"
                "- Módulo de **performance tuning** com queries do mundo real\n"
                "- Administração de PostgreSQL em **produção** (backup, replicação, HA)\n"
                "- Bônus: integração com **Python**, **Node.js** e **.NET**\n\n"
                "> *O banco de dados mais avançado do mundo open source — aprenda a usá-lo de verdade.*"
            ),
        },
        {
            "name": "MongoDB e NoSQL na Prática",
            "price": 197.00,
            "discount": 15,
            "featured": False,
            "description": (
                "## MongoDB e NoSQL na Prática\n\n"
                "Aprenda **MongoDB** e o paradigma **NoSQL** — modelagem de documentos, "
                "aggregation pipeline, índices e deploy em cluster com replica sets.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos NoSQL | Documentos, coleções, BSON, schema flexível |\n"
                "| CRUD | insertMany, find, update, delete, operadores |\n"
                "| Aggregation | Pipeline, $group, $lookup, $unwind, $facet |\n"
                "| Modelagem | Embedding vs referencing, patterns, anti-patterns |\n"
                "| Performance | Índices, explain, profiler, sharding |\n"
                "| Projeto Final | API Node.js + MongoDB com Mongoose |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 45 horas de conteúdo |\n"
                "| Aulas | 140 vídeo-aulas |\n"
                "| Nível | Iniciante ao Intermediário |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **MongoDB Atlas** gratuito para praticar na nuvem\n"
                "- Módulo completo de **aggregation pipeline** com 30+ exercícios\n"
                "- Comparativo prático **SQL vs NoSQL** — quando usar cada um\n"
                "- Projeto de **API REST** completa com Node.js e Mongoose\n"
                "- Bônus: introdução a **Redis** como cache\n\n"
                "> *Nem tudo é tabela — aprenda quando e como usar documentos para escalar.*"
            ),
        },
        {
            "name": "Redis, Cache e Mensageria",
            "price": 197.00,
            "discount": 0,
            "featured": False,
            "description": (
                "## Redis, Cache e Mensageria\n\n"
                "Domine **Redis** como cache, session store, message broker e "
                "banco de dados em memória — a ferramenta que toda aplicação performática usa.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Strings, hashes, lists, sets, sorted sets |\n"
                "| Cache | TTL, invalidação, cache-aside, write-through |\n"
                "| Pub/Sub | Mensageria em tempo real, channels, patterns |\n"
                "| Streams | Event sourcing, consumer groups, XREAD |\n"
                "| Persistência | RDB, AOF, configuração de durabilidade |\n"
                "| Projeto Final | Sistema de cache + fila de jobs com Redis |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 35 horas de conteúdo |\n"
                "| Aulas | 100 vídeo-aulas |\n"
                "| Nível | Intermediário |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- Exemplos com **Node.js**, **Python** e **.NET** (ioredis, redis-py, StackExchange)\n"
                "- Módulo de **Redis Streams** para event-driven architecture\n"
                "- Implementação de **rate limiter** e **session store**\n"
                "- Deploy de **Redis Sentinel** e **Redis Cluster** com Docker\n"
                "- Bônus: comparativo com **RabbitMQ** e **Kafka**\n\n"
                "> *Milissegundos importam — Redis é a diferença entre uma app rápida e uma app lenta.*"
            ),
        },
    ],
    "Designer": [
        {
            "name": "UI e UX Design com Figma",
            "price": 347.00,
            "discount": 10,
            "featured": True,
            "description": (
                "## UI e UX Design com Figma\n\n"
                "Curso completo de **UI/UX Design** usando **Figma** — da pesquisa com usuários "
                "ao protótipo interativo de alta fidelidade.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| UX Research | Personas, user journey, entrevistas, card sorting |\n"
                "| Information Architecture | Sitemaps, fluxos, wireframes |\n"
                "| UI Design | Tipografia, cores, grid, design system |\n"
                "| Figma Avançado | Auto Layout, variantes, componentes, plugins |\n"
                "| Prototyping | Interações, animações, smart animate |\n"
                "| Handoff | Dev mode, CSS, exportação de assets |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 65 horas de conteúdo |\n"
                "| Aulas | 200 vídeo-aulas |\n"
                "| Nível | Iniciante ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **Design System** completo construído do zero (componentes, tokens, documentação)\n"
                "- **5 projetos reais**: landing page, app mobile, dashboard, e-commerce, SaaS\n"
                "- Módulo de **Dev Handoff** — como entregar designs que devs amam\n"
                "- Templates de **portfólio** no Figma inclusos\n"
                "- Acesso à comunidade de **design review** com feedback entre alunos\n\n"
                "> *Design não é só deixar bonito — é resolver problemas visuais que pessoas reais têm.*"
            ),
        },
        {
            "name": "Photoshop para Iniciantes",
            "price": 197.00,
            "discount": 20,
            "featured": False,
            "description": (
                "## Photoshop para Iniciantes\n\n"
                "Aprenda **Adobe Photoshop** do zero — edição de fotos, manipulação de imagens, "
                "criação de artes para redes sociais e tratamento profissional.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Interface | Workspace, painéis, atalhos essenciais |\n"
                "| Seleção | Laço, varinha mágica, seleção por cor, máscara |\n"
                "| Camadas | Blending modes, masks, smart objects, estilos |\n"
                "| Retoque | Healing brush, clone stamp, frequency separation |\n"
                "| Composição | Montagens, recorte profissional, perspectiva |\n"
                "| Social Media | Templates para Instagram, YouTube, banners |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 40 horas de conteúdo |\n"
                "| Aulas | 130 vídeo-aulas |\n"
                "| Nível | Iniciante |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **50+ templates** de social media inclusos (editáveis em PSD)\n"
                "- Módulo de **retoque de pele** profissional (frequency separation)\n"
                "- **Actions e presets** prontos para acelerar o workflow\n"
                "- Exercícios práticos com **antes/depois** para cada técnica\n"
                "- Funciona com **Photoshop CC 2024** e versões anteriores\n\n"
                "> *A ferramenta que todo designer precisa dominar — comece agora mesmo, sem experiência prévia.*"
            ),
        },
        {
            "name": "Motion Design com After Effects",
            "price": 397.00,
            "discount": 0,
            "featured": False,
            "description": (
                "## Motion Design com After Effects\n\n"
                "Domine **animação e motion graphics** com **Adobe After Effects** — "
                "de animações básicas a composições complexas com expressões e 3D.\n\n"
                "### Conteúdo Programático\n\n"
                "| Módulo | Tópicos |\n"
                "|--------|---------|\n"
                "| Fundamentos | Keyframes, easing, princípios de animação |\n"
                "| Shape Layers | Morphing, trim paths, repeater, animação de ícones |\n"
                "| Tipografia | Text animators, kinetic typography, lyrics videos |\n"
                "| Composição | Masks, track mattes, blending, chroma key |\n"
                "| Expressões | Wiggle, loop, time, Math, condicionais |\n"
                "| 3D e Plugins | Câmera 3D, luzes, Element 3D, Lottie export |\n\n"
                "### Detalhes\n\n"
                "| Item | Detalhe |\n"
                "|------|---------|\n"
                "| Duração | 75 horas de conteúdo |\n"
                "| Aulas | 220 vídeo-aulas |\n"
                "| Nível | Iniciante ao Avançado |\n"
                "| Certificado | Sim, com verificação digital |\n"
                "| Acesso | Vitalício |\n\n"
                "### Destaques\n\n"
                "- **20 projetos animados** do início ao fim (intros, lower thirds, social media)\n"
                "- Módulo de **expressões** que elimina 80% do trabalho repetitivo\n"
                "- Export para **Lottie/Bodymovin** para animações web e mobile\n"
                "- **Project files** de todos os projetos inclusos\n"
                "- Bônus: módulo de **Cinema 4D Lite** integrado ao After Effects\n\n"
                "> *Dê vida ao estático — motion design é a skill que transforma bons designs em experiências memoráveis.*"
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
    print(">> Criando store 'Plataforma de Cursos'...")
    resp = requests.post(
        f"{LOFN_URL}/store/insert",
        json={"name": "Plataforma de Cursos"},
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
            "productType": PRODUCT_TYPE_INFO,
            "featured": product.get("featured", False),
        },
        headers={**COMMON_HEADERS, "Authorization": f"Bearer {token}"},
    )
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
        f"Modern online course thumbnail for '{product_name}', "
        "clean flat design, gradient background, tech icons, "
        "professional education style, high quality, vibrant colors"
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

    for category_name, products in CATEGORIES.items():
        print(f"\n>> Subcategoria: {ROOT_CATEGORY}/{category_name}")
        category_id = get_or_create_global_category(token, category_name, parent_id=root_category_id)

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


if __name__ == "__main__":
    main()
