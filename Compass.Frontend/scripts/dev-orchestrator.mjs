// scripts/dev-orchestrator.mjs
import { spawn, execSync } from 'child_process';
import net from 'net';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

// --- 1. ESCUDO ANTI-LOOP (RECURSION GUARD) ---
if (process.env.COMPASS_ORCHESTRATOR_ACTIVE === 'true') {
  process.exit(0);
}
process.env.COMPASS_ORCHESTRATOR_ACTIVE = 'true';
// ---------------------------------------------

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const COLORS = {
  reset: '\x1b[0m',
  sys: '\x1b[1;35m[SYS]\x1b[0m',   // Magenta Bold
  api: '\x1b[1;36m[API]\x1b[0m',   // Ciano Bold
  vue: '\x1b[1;32m[VUE]\x1b[0m',   // Verde Bold
  err: '\x1b[1;31m[ERR]\x1b[0m',   // Vermelho Bold
  warn: '\x1b[1;33m[WARN]\x1b[0m'  // Amarelo Bold
};

// 2. Descoberta Inteligente da Raiz do Workspace
function findWorkspaceRoot() {
  let current = path.resolve(__dirname, '..');
  if (!fs.existsSync(path.join(current, 'Compass.Backend'))) {
    current = path.resolve(__dirname, '..', '..');
  }
  return current;
}

// 3. CAÇADOR DE ZUMBIS: Limpa instâncias presas na memória antes do boot
function killZombieProcesses() {
  try {
    if (process.platform === 'win32') {
      // O comando taskkill no Windows encerra silenciosamente o processo preso (ex: PID 16900)
      execSync('taskkill /f /im Compass.Api.exe 2>nul', { stdio: 'ignore' });
    } else {
      execSync('pkill -f Compass.Api 2>/dev/null', { stdio: 'ignore' });
    }
    console.log(`${COLORS.sys} Processos antigos do backend em segundo plano foram limpos.`);
  } catch (e) {
    // Se não havia nenhum zumbi rodando, o comando falha silenciosamente e a vida segue normal
  }
}

// 4. Verificação de Saúde da Conexão com o PostgreSQL
function checkPostgresConnection(host = '127.0.0.1', port = 5432, timeoutMs = 3000) {
  return new Promise((resolve) => {
    const socket = new net.Socket();
    let status = false;

    socket.setTimeout(timeoutMs);
    socket.on('connect', () => {
      status = true;
      socket.destroy();
    });
    socket.on('timeout', () => socket.destroy());
    socket.on('error', () => socket.destroy());
    socket.on('close', () => resolve(status));

    socket.connect(port, host);
  });
}

// 5. Disparo de Processos Compatível com Node v24 (Sem DEP0190)
function spawnService(name, prefixColor, commandString, cwd) {
  if (!fs.existsSync(cwd)) {
    console.error(`\n${COLORS.err} Erro Fatal: A pasta para '${name}' não foi encontrada em: ${cwd}\n`);
    process.exit(1);
  }

  const child = spawn(commandString, [], { 
    cwd, 
    shell: true, 
    stdio: 'pipe',
    env: { ...process.env, COMPASS_ORCHESTRATOR_ACTIVE: 'true' }
  });

  child.stdout.on('data', (data) => {
    const lines = data.toString().trim().split('\n');
    for (const line of lines) {
      if (line) console.log(`${prefixColor} ${line}`);
    }
  });

  child.stderr.on('data', (data) => {
    const lines = data.toString().trim().split('\n');
    for (const line of lines) {
      if (line && !line.includes('warning CS')) {
        console.error(`${COLORS.err} ${prefixColor} ${line}`);
      }
    }
  });

  child.on('close', (code) => {
    if (code !== 0 && code !== null) {
      console.log(`${COLORS.warn} O serviço ${name} foi encerrado com código ${code}.`);
    }
  });

  return child;
}

// 6. Orquestração Principal
async function bootstrap() {
  console.clear();
  console.log(`${COLORS.sys} ======================================================`);
  console.log(`${COLORS.sys}   COMPASS LOCAL-FIRST ENGINE — DEV ORCHESTRATOR v1.0`);
  console.log(`${COLORS.sys} ======================================================\n`);

  const rootDir = findWorkspaceRoot();
  console.log(`${COLORS.sys} Workspace detectado em: ${rootDir}`);
  console.log(`${COLORS.sys} Verificando disponibilidade do PostgreSQL (Porta 5432)...`);
  
  const isDbOnline = await checkPostgresConnection();

  if (!isDbOnline) {
    console.log(`\n${COLORS.err} ================= [ FALHA DE AMBIENTE ] =================`);
    console.log(`${COLORS.err} Não foi possível conectar ao PostgreSQL em 127.0.0.1:5432.`);
    console.log(`${COLORS.err} Por favor, verifique se o serviço local ou o contêiner Docker está rodando.`);
    console.log(`${COLORS.err} =========================================================\n`);
    process.exit(1);
  }

  console.log(`${COLORS.sys} PostgreSQL detectado! Limpando processos zumbis e iniciando...`);
  
  // RODA O CAÇADOR DE ZUMBIS ANTES DE SUBIR A API!
  killZombieProcesses();
  console.log('');

  const apiPath = path.join(rootDir, 'Compass.Backend', 'src', 'Compass.Api');
  const vuePath = path.join(rootDir, 'Compass.Frontend');

  // Subindo Kestrel (.NET 10)
  const apiProcess = spawnService('Backend API', COLORS.api, 'dotnet run', apiPath);

  // Subindo Vite (Vue 3) — Execução direta
  const vueProcess = spawnService('Frontend Vue', COLORS.vue, 'npx vite --port 5173', vuePath);

  // 7. Encerramento Gracioso em Cascata
  const shutdown = () => {
    console.log(`\n${COLORS.sys} Encerrando ecossistema Compass...`);
    apiProcess.kill('SIGINT');
    vueProcess.kill('SIGINT');
    killZombieProcesses(); // Garante limpeza ao fechar!
    process.exit(0);
  };

  process.on('SIGINT', shutdown);  
  process.on('SIGTERM', shutdown); 
}

bootstrap();