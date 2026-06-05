#!/bin/bash

# 途中でエラーが発生した場合、スクリプトを即座に終了する
set -e

# root権限（sudo）で実行されているか確認
if [ "$EUID" -ne 0 ]; then
  echo "エラー: このスクリプトは root 権限で実行する必要があります。sudo を付けて実行してください。" >&2
  exit 1
fi

echo "WSL の DNS 設定ファイル (/etc/wsl.conf) の自動生成を無効化中..."
cat << 'EOF' >> /etc/wsl.conf
[network]
generateResolvConf = false
EOF

echo "WSL の DNS 設定ファイル (/etc/resolv.conf) を作成中..."
rm -f /etc/resolv.conf
cat << 'EOF' > /etc/resolv.conf
nameserver 8.8.8.8
nameserver 1.1.1.1
options timeout:2 attempts:3
EOF

# echo "AlmaLinux をアップデート..."
# dnf update -y
# dnf upgrade -y

echo "日本語言語パックをインストール..."
dnf install -y glibc-langpack-ja

echo "タイムゾーンを Asia/Tokyo に設定..."
timedatectl set-timezone Asia/Tokyo

echo "ロケールを ja_JP.UTF-8 に設定..."
localectl set-locale LANG=ja_JP.UTF-8

echo "ユーザー dev を作成中..."
useradd -m -s /bin/bash dev

echo "ユーザー dev に sudo 権限を設定中..."
echo "dev ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/dev
chmod 0440 /etc/sudoers.d/dev

echo "WSL のデフォルトログインユーザーをユーザー dev に設定中..."
cat << 'EOF' >> /etc/wsl.conf
[user]
default=dev
EOF

echo "WSL のデフォルトユーザー作成機能を無効化中..."
sed -i -E 's|^[[:space:]]*command[[:space:]]*=[[:space:]]*/usr/lib/wsl/oobe[[:space:]]*$|# command = /usr/lib/wsl/oobe|' "/etc/wsl-distribution.conf"

echo "一般ユーザーが ping コマンドを使用できるように設定中..."
mkdir -p /etc/sysctl.d
tee /etc/sysctl.d/99-ping.conf >/dev/null <<'EOF'
net.ipv4.ping_group_range = 0 2147483647
EOF
sysctl --system

echo "git をインストール中..."
dnf install -y git
git --version

echo "docker をインストール中..."
dnf remove  docker \
            docker-client \
            docker-client-latest \
            docker-common \
            docker-latest \
            docker-latest-logrotate \
            docker-logrotate \
            docker-engine \
            podman \
            runc
dnf install -y dnf-utils
dnf config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
dnf install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
systemctl enable --now docker
docker --version

echo "ユーザー dev を docker グループに追加中..."
usermod -aG docker dev

echo "compose.yml で Docker コンテナを構築中..."
docker compose up -d

echo "AWS CLI をインストール中..."
(
  cd /tmp && \
  curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
  unzip ./awscliv2.zip && \
  ./aws/install
  rm -rf ./awscliv2.zip ./aws
)

echo "ユーザー dev の .bashrc に AWS モックへの接続情報を追加中..."
cat << 'EOF' >> /home/dev/.bashrc

# AWS LocalSettings (floci etc.)
export AWS_ENDPOINT_URL=http://localhost:4566
export AWS_DEFAULT_REGION=ap-northeast-1
export AWS_ACCESS_KEY_ID=DUMMY
export AWS_SECRET_ACCESS_KEY=DUMMY
EOF

echo "設定完了"