# *곧 새로운 저장소로 이관될 예정입니다.*

[![Stories in Ready](https://badge.waffle.io/envicase/flip.png?label=ready&title=Ready)](https://waffle.io/envicase/flip)
# Flip

envicase([www.envicase.com](https://www.envicase.com)) 개발팀은 서비스의 iOS 클라이언트 응용프로그램을 개발하면서 여러 뷰에서 보여지는 동일한 컨텐트의 상태를 동기화하기 위한 단순하고 효율적인 방법을 고민했고, Rx(Reactive Extensions)를 사용하여 반응형 모델 스트림 기반의 새로운 아키텍처를 만들었습니다. Flip은 반응형 모델 스트림과 MVVM(Model-View-ViewModel)과 같은 응용프로그램 디자인 패턴 및 프레임워크를 지원합니다.

## 반응형 모델 스트림

응용프로그램에서 모델과 뷰는 일대다 관계를 가집니다. 예를 들어 master-detail 인터페이스를 가진 응용프로그램의 경우 마스터 목록의 선택된 항목의 뷰와 상세 뷰는 동일한 모델을 표현합니다. 하지만 동일한 모델을 표현한다고 해서 두 뷰가 동일한 기능을 제공하는 것은 아닙니다. 상세 뷰는 항목 뷰가 보여주지 않는 속성을 추가적으로 보여주기도 하고 항목 속성에 대한 편집 기능을 제공할 수도 있습니다.

```text
+---------------+--------------------+
| MASTER        | DETAIL             |
+---------------+--------------------+
| Contact 1     | Id   : 2           |
| Contact 2   < | Name : Hello World |
| Contact 3     | Email: foo@bar.com |
| Contact 4     +--------------------|
|               |        Save        |
+---------------+--------------------+
```

이 때 상세 뷰를 통해 모델의 속성을 수정하면 동일한 모델을 표현하는 다른 뷰들은 어떻게 갱신해야하는지 고민해야합니다. Flip은 반응형 모델 스트림을 제공해 이 문제 해결을 도와줍니다.

### 모델 스트림 구독

Flip은 식별자(`Id` 속성)를 통해 모델을 식별하며 모델 구현을 위한 `IModel<TId>` 인터페이스와 `Model<TId>` 추상 클래스를 제공합니다. 연락처를 관리하는 모델이라면 다음과 같이 작성될 수 있습니다.

```csharp
public class Contact : Model<int>
{
    public Contact(int id, string name, string email)
        : base(id)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; }
    public string Email { get; }
}
```

뷰를 처리하는 코드는 스트림에 연결하여 모델 스트림을 구독합니다. 스트림에 연결하면 `IConnection<TModel, TId>` 인스턴스가 반환되며 `IConnection<TModel, TId>` 인터페이스는 `IObservable<TModel>` 인터페이스를 상속받기 때문에 `Subscribe()` 메서드를 통해 구독될 수 있으며 Rx 연산을 사용할 수 있습니다. 만약 응용프로그램이 MVVM 패턴을 사용한다면 뷰모델은 스트림을 구독하고 속성을 사용하여 모델을 뷰에 노출합니다.

```csharp
public class ContactViewModel : ViewModelBase
{
    private Contact _model;

    public ContactViewModel(Contact user)
    {
        Connection = Stream<Contact, int>.Connect(user.Id);
        Connection.Subscribe(m => Model = m);
    }

    protected IConnection<Contact, int> Connection { get; }

    public Contact Model
    {
        get { return _model; }
        private set { Set(ref _model, value); }
    }
}
```

`ContactViewModel`에 대한 다음과 같은 뷰(XAML) 코드가 있을 때 뷰는 응용프로그램의 모든 곳에서 발생하는 모델 변화를 반영하게 됩니다.

```xaml
<TextBlock Text="{Binding Model.Name, Mode=OneWay}" />
<TextBlock Text="{Binding Model.Email, Mode=OneWay}" />
```

### 새로운 모델 개정 발행

모델의 변경 역시 스트림 연결을 통해 처리할 수 있습니다. `IConnection<TModel, TId>` 인터페이스가 정의하는 `void Emit(IObservable<TModel> source)` 메서드와 `ConnectionExtensions` 클래스가 제공하는 확장 메서드를 사용하여 새로운 모델의 개정 인스턴스를 발행할 수 있습니다. 예를 들어 연락처를 수정하는 기능을 가진 뷰모델을 추가한다면 다음과 같이 `SaveCommand` 명령을 작성할 수 있습니다.

```csharp
public class ContactEditorViewModel : ContactViewModel
{
    private string _editName;
    private string _editEmail;

    public ContactEditorViewModel(Contact user)
        : base(user.Id)
    {
        _editName = user.Name;
        _editEmail = user.Email;
    }

    public string EditName
    {
        get { return _editName; }
        set { Set(ref _editName, value); }
    }

    public string EditEmail
    {
        get { return _editEmail; }
        set { Set(ref _editEmail, value); }
    }

    public ICommand SaveCommand => new RelayCommand(() =>
        Connection.Emit(new Contact(Model.Id, _editName, _editEmail)));
}
```

이제 동일한 모델을 표현하는 응용프로그램의 모든 뷰는 모델의 새로운 상태를 반영합니다. 모델의 상태를 변경하는 뷰모델과 상태 변경을 구독하는 뷰모델은 서로 어떠한 연관도 가질 필요가 없기 때문에 약한 결합도를 가지거나 결합도를 전혀 가지지 않습니다.
