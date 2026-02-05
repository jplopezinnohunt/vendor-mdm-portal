import { execSync } from 'child_process';
import { writeFileSync, readFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

/**
 * Generates version information from git and writes it to src/version.ts
 */
function generateVersion() {
  try {
    // Fix git safe.directory for CI environments (Azure Static Web Apps uses Docker)
    try {
      execSync('git config --global --add safe.directory /github/workspace', { encoding: 'utf-8', stdio: 'pipe' });
    } catch {
      // Ignore - not in CI or already configured
    }

    // Get version from package.json
    const packageJsonPath = join(__dirname, 'package.json');
    const packageJson = JSON.parse(readFileSync(packageJsonPath, 'utf-8'));
    const pkgVersion = packageJson.version || '0.0.0';

    // Get the last commit date
    let lastCommitDate = '';
    try {
      lastCommitDate = execSync('git log -1 --format=%ci', { encoding: 'utf-8', stdio: 'pipe' }).trim();
    } catch {
      lastCommitDate = new Date().toISOString();
    }

    // Get the last commit hash (short)
    let commitHash = '';
    try {
      commitHash = execSync('git rev-parse --short HEAD', { encoding: 'utf-8', stdio: 'pipe' }).trim();
    } catch {
      commitHash = (process.env.GITHUB_SHA || 'unknown').substring(0, 7);
    }

    // Get the total number of commits (still useful for internal stats, but not for display version)
    let commitCount = 0;
    try {
      commitCount = parseInt(execSync('git rev-list --count HEAD', { encoding: 'utf-8', stdio: 'pipe' }).trim(), 10);
    } catch {
      commitCount = 0;
    }

    // Get the current branch name
    let branch = '';
    try {
      branch = execSync('git rev-parse --abbrev-ref HEAD', { encoding: 'utf-8', stdio: 'pipe' }).trim();
    } catch {
      branch = process.env.GITHUB_REF_NAME || 'unknown';
    }

    // Format the build date
    const buildDate = new Date().toISOString();

    // Create the version file content
    const versionContent = `// This file is auto-generated. Do not edit manually.
// Generated on: ${buildDate}

export interface VersionInfo {
  version: string;
  commitCount: number;
  commitHash: string;
  branch: string;
  buildDate: string;
  lastCommitDate: string;
}

export const version: VersionInfo = {
  version: 'v${pkgVersion}',
  commitCount: ${commitCount},
  commitHash: '${commitHash}',
  branch: '${branch}',
  buildDate: '${buildDate}',
  lastCommitDate: '${lastCommitDate}',
};


export default version;
`;

    // Write to src/version.ts
    const outputPath = join(__dirname, 'src', 'version.ts');
    writeFileSync(outputPath, versionContent, 'utf-8');

    console.log(`✅ Version file generated successfully!`);
    console.log(`   Version: v${commitCount}`);
    console.log(`   Commit: ${commitHash}`);
    console.log(`   Branch: ${branch}`);
    console.log(`   Last Commit: ${lastCommitDate}`);
    console.log(`   Build Date: ${buildDate}`);

  } catch (error) {
    console.error('❌ Error generating version file:', error.message);

    // Create a fallback version file for non-git environments
    const fallbackContent = `// This file is auto-generated. Do not edit manually.
// Fallback version (git not available)

export interface VersionInfo {
  version: string;
  commitCount: number;
  commitHash: string;
  branch: string;
  buildDate: string;
  lastCommitDate: string;
}

export const version: VersionInfo = {
  version: 'v0.0.0',
  commitCount: 0,
  commitHash: 'unknown',
  branch: 'unknown',
  buildDate: '${new Date().toISOString()}',
  lastCommitDate: 'unknown',
};

export default version;
`;

    const outputPath = join(__dirname, 'src', 'version.ts');
    writeFileSync(outputPath, fallbackContent, 'utf-8');
    console.log('⚠️  Fallback version file created (git not available)');
  }
}

// Run the function
generateVersion();
