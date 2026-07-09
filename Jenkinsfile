pipeline {

    agent any

    environment {
        PATH = "/Users/williamgiftson/.dotnet/tools:/usr/local/share/dotnet:${env.PATH}"
        SONAR_TOKEN = credentials('sonar-token')
    }

    stages {

        stage('Checkout') {
            steps {
                git branch: 'main',
                    credentialsId: 'github-creds',
                    url: 'https://github.com/WLiam18/RoadReady.git'
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore RoadReady.slnx'
            }
        }

        stage('Check Tools') {
            steps {
                sh '''
                echo "Checking environment..."

                whoami

                echo "Dotnet location:"
                which dotnet

                echo "SonarScanner location:"
                which dotnet-sonarscanner

                echo "Dotnet version:"
                dotnet --version

                echo "SonarScanner version:"
                dotnet sonarscanner --version || true
                '''
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    sh '''
                    echo "Starting SonarQube Analysis..."

                    dotnet sonarscanner begin \
                    /k:"RoadReady" \
                    /d:sonar.host.url="http://localhost:9000" \
                    /d:sonar.login=$SONAR_TOKEN

                    dotnet build RoadReady.slnx

                    dotnet sonarscanner end \
                    /d:sonar.login=$SONAR_TOKEN
                    '''
                }
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test RoadReady.slnx'
            }
        }
    }
}