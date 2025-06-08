# Command Center API

A powerful ASP.NET Core API service that executes shell commands remotely with elevated privileges. This service is designed to run commands on Linux servers, making it ideal for deployment automation, server management, and CI/CD workflows.

## What It Does

Command Center provides a REST API endpoint that:
- Executes multiple shell commands in a specified directory
- Runs with elevated privileges (root) for system administration tasks
- Returns both standard output and error streams
- Perfect for automating deployments, running migrations, managing git repositories, and other server maintenance tasks

## Features

- 🚀 Execute multiple commands in sequence
- 📁 Change working directory before execution
- 📝 Capture all output and errors
- 🔧 Built with ASP.NET Core 9.0
- 🐧 Linux-focused (uses `/bin/bash`)
- 🔄 Auto-restart on failure via systemd

## API Usage

### Endpoint
```
POST http://your-server:7262/commands/run
```

### Request Body
```json
{
  "path": "/var/www/your-app",
  "commands": [
    "git pull",
    "dotnet restore",
    "dotnet build",
    "systemctl restart your-app"
  ]
}
```

### Response
```json
{
  "success": true,
  "output": "$ git pull\nAlready up to date.\n$ dotnet restore\nRestore completed...",
  "error": ""
}
```

## Deployment Guide

### Prerequisites

1. Ubuntu/Debian Linux server
2. Root or sudo access
3. .NET 9.0 Runtime

### Step 1: Install .NET 9.0

```bash
# Add Microsoft package repository
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update

# Install .NET SDK and Runtime
sudo apt-get install -y dotnet-sdk-9.0
sudo apt-get install -y aspnetcore-runtime-9.0
```

### Step 2: Build and Deploy the Application

On your development machine:
```bash
# Build the project
dotnet publish -c Release -o ./publish

# Copy to server
scp -r ./publish/* user@your-server:/var/command/
```

### Step 3: Create systemd Service

Create the service file:
```bash
sudo nano /etc/systemd/system/command-center.service
```

Add the following content:
```ini
[Unit]
Description=API Command Center for running elevated commands in Webserver Applications
After=network.target

[Service]
WorkingDirectory=/var/command
ExecStart=dotnet /var/command/"Command Center.dll"

Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=command-center
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:7262
Environment=HOME=/root
Environment=USER=root
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### Step 4: Enable and Start the Service

```bash
# Reload systemd configuration
sudo systemctl daemon-reload

# Enable service to start on boot
sudo systemctl enable command-center

# Start the service
sudo systemctl start command-center

# Check service status
sudo systemctl status command-center

# View logs
sudo journalctl -u command-center -f
```

### Step 5: Configure Firewall (Optional)

If you need external access:
```bash
# Open port 7262
sudo ufw allow 7262/tcp
```

## Security Considerations

⚠️ **WARNING**: This service runs as root and executes arbitrary commands. 

### Recommended Security Measures:

1. **Network Security**
   - Only bind to localhost (default configuration)
   - Use a reverse proxy (nginx) with authentication
   - Implement IP whitelisting

2. **API Security**
   - Add authentication middleware
   - Implement API key validation
   - Use HTTPS in production

3. **Command Validation**
   - Whitelist allowed commands
   - Validate paths
   - Add command timeouts

## Example: Nginx Reverse Proxy with Basic Auth

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location /command-center/ {
        auth_basic "Command Center";
        auth_basic_user_file /etc/nginx/.htpasswd;
        
        proxy_pass http://localhost:7262/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Common Use Cases

### 1. Git Deployment
```json
{
  "path": "/var/www/myapp",
  "commands": [
    "git config --global --add safe.directory /var/www/myapp",
    "git pull origin main",
    "npm install",
    "npm run build",
    "pm2 restart myapp"
  ]
}
```

### 2. Database Migrations
```json
{
  "path": "/var/www/myapp",
  "commands": [
    "php spark migrate --force",
    "php spark cache:clear",
  ]
}
```

### 3. Docker Management
```json
{
  "path": "/home/docker/myapp",
  "commands": [
    "docker-compose pull",
    "docker-compose up -d",
    "docker system prune -f"
  ]
}
```

## Troubleshooting

### Service won't start
```bash
# Check logs
sudo journalctl -u command-center -n 50

# Verify .NET installation
dotnet --info

# Check file permissions
ls -la /var/command/
```

### Permission errors with git
```bash
# Add safe directory
git config --global --add safe.directory /path/to/repo

# Fix ownership
chown -R root:root /path/to/repo/.git
```

### Port already in use
```bash
# Find process using port
sudo netstat -tlnp | grep 7262

# Change port in service file
Environment=ASPNETCORE_URLS=http://localhost:7263
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

We welcome contributions to Command Center! This project is open source and community-driven.

### How to Contribute

1. **Fork the Repository**
   ```bash
   git clone https://github.com/MEITWorld/Command-Center.git
   cd command-center
   ```

2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make Your Changes**
   - Write clean, documented code
   - Follow C# coding conventions
   - Add unit tests for new features
   - Update documentation as needed

4. **Test Your Changes**
   ```bash
   dotnet test
   dotnet run
   ```

5. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "Add feature: description of your changes"
   ```

6. **Push to Your Fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request**
   - Go to the original repository
   - Click "New Pull Request"
   - Select your feature branch
   - Describe your changes in detail

### Contribution Guidelines

#### Code Style
- Use C# naming conventions
- Keep methods small and focused
- Add XML documentation comments for public APIs
- Use meaningful variable and method names

#### Testing
- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Aim for good code coverage

#### Documentation
- Update README.md for new features
- Add inline comments for complex logic
- Include examples for new functionality

#### Pull Request Process
1. Ensure your PR description clearly describes the problem and solution
2. Include the relevant issue number if applicable
3. Update the README.md with details of changes to the interface
4. Your PR will be reviewed by maintainers
5. Make requested changes if any
6. Once approved, your PR will be merged

### Areas for Contribution

- **Security Enhancements**: Authentication, authorization, rate limiting
- **Features**: Command queuing, async execution, webhooks
- **Documentation**: Tutorials, examples, translations
- **Testing**: Unit tests, integration tests, performance tests
- **Bug Fixes**: Check the issues page for reported bugs

### Reporting Issues

Found a bug or have a feature request? Please create an issue:

1. Check if the issue already exists
2. Use a clear and descriptive title
3. Provide detailed steps to reproduce (for bugs)
4. Include system information (OS, .NET version)
5. Add relevant logs or error messages

### Code of Conduct

Please note that this project follows a Code of Conduct. By participating, you are expected to:

- Be respectful and inclusive
- Welcome newcomers and help them get started
- Focus on constructive criticism
- Respect differing viewpoints and experiences

### Getting Help

- 📧 Email: [software@nexgen.co.zm]

Thank you for contributing to Command Center! 🚀