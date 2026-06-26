# ConfigEditor

ConfigEditor는 JSON, INI, CFG 설정 파일을 열어 구조를 확인하고 값을 편집할 수 있는 Windows 데스크톱 애플리케이션입니다. 원문 편집과 폼 기반 편집을 함께 제공하여 설정 파일을 직접 수정하면서도 문서 구조와 검사 결과를 확인할 수 있습니다.

## 주요 기능

- JSON, INI, CFG 파일 열기 및 저장
- 여러 설정 파일 동시 열기
- 최근 파일 목록 관리
- 설정 문서 구조 트리 표시
- 원문 편집 탭과 폼 편집 탭 제공
- JSON / INI 문서 검사 및 오류 표시
- 저장 전 유효성 검사
- 수동 백업 생성 및 백업 폴더 열기
- 폴더 단위 설정 파일 조사
- 밝은 화면 / 어두운 화면 테마 전환
- 상태바에서 현재 앱 버전 표시

## 지원 형식

| 형식 | 확장자 | 설명 |
| --- | --- | --- |
| JSON | `.json` | 객체, 배열, 문자열, 숫자, Boolean, Null 값을 구조화해서 편집합니다. |
| INI | `.ini`, `.cfg` | 섹션과 키-값 항목을 구조화해서 편집합니다. |

## 실행 환경

- Windows
- .NET 10 SDK
- WPF 지원 환경

## 빌드

루트 디렉터리에서 다음 명령을 실행합니다.

```powershell
dotnet build ConfigEditor.slnx
```

앱 프로젝트만 빌드하려면 다음 명령을 사용할 수 있습니다.

```powershell
dotnet build ConfigEditor.App\ConfigEditor.App.csproj
```

## 테스트

```powershell
dotnet test ConfigEditor.Tests\ConfigEditor.Tests.csproj
```

## 프로젝트 구조

```text
ConfigEditor.App             WPF 애플리케이션, 화면, ViewModel, 앱 서비스
ConfigEditor.Core            공통 모델, 인터페이스, 결과 타입
ConfigEditor.Formats         JSON / INI 파서, 작성기, 유효성 검사기
ConfigEditor.Infrastructure  파일, 백업, 로그 서비스 구현
ConfigEditor.Tests           파서, 검사, 백업 서비스 테스트
```

## 사용 흐름

1. `파일 열기`로 JSON, INI, CFG 파일을 선택합니다.
2. 왼쪽 목록에서 열린 파일을 선택합니다.
3. `원문 편집` 또는 `폼 편집` 탭에서 값을 수정합니다.
4. `검사`로 문서 오류를 확인합니다.
5. 문제가 없으면 `저장` 또는 `다른 이름으로 저장`을 실행합니다.

## 버전

현재 앱 버전은 `v1.0.0`입니다.
