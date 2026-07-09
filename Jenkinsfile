pipeline {
    agent any

    environment {
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
                sh 'dotnet restore RoadReady.sln'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build RoadReady.sln --no-restore'
            }
        }

        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    sh '''
                    dotnet sonarscanner begin \
                    /k:"RoadReady" \
                    /d:sonar.host.url="http://localhost:9000" \
                    /d:sonar.login=$SONAR_TOKEN

                    dotnet build RoadReady.sln

                    dotnet sonarscanner end \
                    /d:sonar.login=$SONAR_TOKEN
                    '''
                }
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test RoadReady.sln'
            }
        }
    }
}
