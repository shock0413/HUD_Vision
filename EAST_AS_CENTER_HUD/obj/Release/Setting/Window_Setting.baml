   M S B A M L   `   `   `  ���� @  <HTool, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null>+clr-namespace:HTool.Template;assembly=HToolHTool.Template  M IEAST_AS_CENTER_HUD, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:EAST_AS_CENTER_HUD.SettingEAST_AS_CENTER_HUD.Setting I EHanseroDisplay, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG4clr-namespace:HanseroDisplay;assembly=HanseroDisplayHanseroDisplay c _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity F BCalcBinding, Version=2.5.2.0, Culture=neutral, PublicKeyToken=null>.clr-namespace:CalcBinding;assembly=CalcBindingCalcBinding /   )EAST_AS_CENTER_HUD.Setting.Window_Setting   8Template+clr-namespace:HTool.Template;assembly=HTool  H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null6mah,http://metro.mahapps.com/winfx/xaml/controls  \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R	 NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml 	    @d2http://schemas.microsoft.com/expression/blend/2008 	    Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006 	    :local(clr-namespace:EAST_AS_CENTER_HUD.Setting 	    Ohanserodisplay4clr-namespace:HanseroDisplay;assembly=HanseroDisplay 	    ^iPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity 	    <c.clr-namespace:CalcBinding;assembly=CalcBinding 	    -     � Title$  세팅��$	��800��$
��1200�� � WindowStartupLocation$ CenterOwner=� � WindowState$ 	Maximized=� � DataContext �� 8  2MahApps.Metro.Controls.Dialogs.DialogParticipation   Register �� *SettingEngine+ � 	Resources  �� 
TargetType �� Property �� Value��%-   '��       �� " ��.���  � ��$ 15(�.���  �� *SettingEngine+	 � ColumnDefinitions	 �� $��4* ��� $��1* �.+�� $��0��
 � RowDefinitions
 �� $
��auto ��� $��4* ��� $��3* �.+���  �� Orientation$ 
Horizontal=�$��5q�.+��� $��차종��$��Center=��� $	��150������ *ListStructCarkind+ �� SelectedItem �� *SelectedCarkind+�� $��검사항목��$��Center=��� $	��150������ *ListStructInspection+ �� *SelectedInspection+� $��1��{��� *IsEnabledDisplay+.+�� 
 �� 	 �� �� .+��� $��0��$��마스터 이미지��+  %MahApps.Metro.Controls.ControlsHelper   HeaderFontSize$ 15Q�.��� 
 �� �� $
��auto �	 �� �� .+�  HanseroDisplay.HDisplay  -   $��3��   BitmapImage �� *MasterImage+ �� Mode$ TwoWay=�   Result �� *MasterResult+$ TwoWay=��� $��1��$��0��$��1������ *SetMasterImageCommand+.���� $ ��마스터 이미지 설정���� $��1��$��1������ *RemoveMasterImageCommand+.���� $ ��마스터 이미지 제거���� $��1��$��검사 이미지��$ 15Q�.��� 
 �� �� $
��auto �	 �� �� �� .+�� $��3��.+�   �� *Image+$ TwoWay=� �� *Result+$ TwoWay=�   IsShowRectangle �� *IsShowRectangle+$ TwoWay=�   IsShowCircle �� *IsShowCircle+$ TwoWay=�   	Confirmed �� *ConfirmCommand+   SelectRectangle �� *SelectRectangle+$ TwoWay=�   SelectCircle �� *SelectCircle+$ TwoWay=��� $	��Top=�$��Right=�$ Vertical=�.+��� ���� *
MainResult+���� *MainResultColor+�� 
�������
���   �$
��Left=�$	��Top=����� *MainResultCommant+�� $��1��$��0������ *OneShotCommand+.���� $��이미지 촬영���� $��1��$��1������ *OpenImageFileCommand+.���� $��이미지 불러오기���� $��1��$��2������ *RunInspectionCommand+.���� $��수동검사��� $��2��$��5q�{��� *IsEnabledSetting+	 �� �� �� �� .+��� $��
HUD 스펙��$��0��$��5q�$ 15Q�.��� 	 �� �� 
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� �� $
��auto �.+��� $��영상 가로 크기��$��0��*  $MahApps.Metro.Controls.NumericUpDown  $��0��$��1��   StringFormat$ {0:N2}mm��   Interval$
 0.01Q�   
IsReadOnly .    HideUpDownButtons .    Value �� *CurrentHudWidth+�� $��영상 세로 크기��$��1��  $��1��$��1��$ {0:N2}mm��$
 0.01Q� .  .  �� *CurrentHudHeight+�� $��도트 가로 개수��$��2��  $��2��$��1��$ {0:N0}ea�� .  .  �� *CurrentHudHorizentalCount+�� $��도트 세로 개수��$��3��  $��3��$��1��$ {0:N0}ea�� .  .  �� *CurrentHudVerticalCount+�� $��도트 가로 간격��$��4��  $��4��$��1��$ {0:N2}mm��$
 0.01Q� .  .  �� *CurrentHudHorizentalInterval+�� $��도트 세로 간격��$��5��  $��5��$��1��$ {0:N2}mm��$
 0.01Q� .  .  �� *CurrentHudVerticalInterval+�� $��7��$��2��$��5q����� *OpenCarinfoCommand+.���� $��변경���� $��1��$��3��.@��� $��잘림검사��$ 15Q�.��� 
 �� �� $
��auto �.+��� .@��� $ 15Q�$��일반��.��� 	 �� �� �� $
��auto �
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� �� $
��auto �.+��� $��합격 범위 상단��$��0��$��1��  $��0��$��1��$ {0:N2}mm��$
 0.05Q�   InterceptManualEnter .  �� *CurrentPassRangeTop+�� $��합격 범위 하단��$��1��$��1��  $��1��$��1��$ {0:N2}mm��$
 0.05Q� .  �� *CurrentPassRangeBottom+�� $��블랍 최소 값��$��2��$��1��  $��2��$��1��$ {0:N0}px�� .  �� *CurrentCutoffMinBlob+�� $��블랍 최대 값��$��3��$��1��  $��3��$��1��$ {0:N0}px�� .  �� *CurrentCutoffMaxBlob+�� $��밝기 임계 값��$��4��$��1��  $��4��$��1��$ {0:N0}�� .  �� *CurrentCutoffBrightLimit+�� $��이동값 부호 전환��$��5��$��1���� $��5��$��1�� d� 	IsChecked �� *IsReverseCutOffValue+.���� $��부호 변환���� $ 15Q�$��전송 계수��.��� 	 �� �� �� $
��auto �
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� �� $
��auto �.+��� $��일반 계수��$��0��$��1��  $��0��$��1��$ {0:N2}��$
 0.01Q�   Minimum$
 0.01Q�   Maximum$ 2Q� .  �� *!CurrentCutoffTransmissionFactor+�� $��1회차 계수��$��1��$��1��  $��1��$��1��$ {0:N2}��$
 0.01Q�$
 0.01Q�$ 2Q� .  �� *" CurrentCutoffTransmissionFactor1+�� $��2회차 계수��$��2��$��1��  $��2��$��1��$ {0:N2}��$
 0.01Q�$
 0.01Q�$ 2Q� .  �� *" CurrentCutoffTransmissionFactor2+�� $��3회차 계수��$��3��$��1��  $��3��$��1��$ {0:N2}��$
 0.01Q�$
 0.01Q�$ 2Q� .  �� *" CurrentCutoffTransmissionFactor3+�� $��40mm 이상 이동 계수��$��4��$��1��  $��4��$��1��$ {0:N2}��$
 0.01Q�$
 0.01Q�$ 2Q� .  �� *'%CurrentCutoffTransmissionFactorOver40+� $��1��	 �� �� .+��� $��0��$��5q����� *SaveCutoffSetCommand+{��� *IsChangedCutoffParams+.���� $��저장���� $��1��$��5q����� *RestoreCutoffParamsCommand+{��� *IsChangedCutoffParams+.���� $��되돌리기���� $��왜곡검사��$ 15Q�.��� 	 �� �� 
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� �� $
��auto �.+��� $��블랍 최소값��$��0��  $��0��$��1��$ 	{0:N0}pix�� �� *CurrentDistortionMinBlob+�� $��블랍 최대값��$��1��  $��1��$��1��$ 	{0:N0}pix�� �� *CurrentDistortionMaxBlob+�� $��밝기 임계값��$��2��  $��2��$��1��$ {0:N0}��$ 0Q�$	��255�� �� *CurrentDistortionBrightLimit+�� $*��$마스터 도트 세로 MM Per Pixel��$��3��  $��3��$��1��$ 0Q�$ 10Q�$	��255��$
 0.01Q� �� *+)CurrentDistortionMasterVerticalMMPerPixel+�� $*��$마스터 도트 가로 MM Per Pixel��$��4��  $��4��$��1��$ 0Q�$ 10Q�$	��255��$
 0.01Q� �� *-+CurrentDistortionMasterHorizentalMMPerPixel+�� $��6��$��5q����� *SaveDistortionSetCommand+{��� *IsChangedDistortionParams+.���� $��저장���� $��6��$��1��$��5q����� * RestoreDistortionParamsCommand+{��� *IsChangedDistortionParams+.���� $��되돌리기���� $��중심검사��$ 15Q�.��� 	 �� �� 
 �� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� $
��auto ��� �� $
��auto �.+��� $��블랍 최소값��$��0��  $��0��$��1��$ 	{0:N0}pix�� �� *CurrentCenterMinBlob+�� $��블랍 최대값��$��1��  $��1��$��1��$ 	{0:N0}pix�� �� *CurrentCenterMaxBlob+�� $��밝기 임계값��$��2��  $��2��$��1��$ {0:N0}��$ 0Q�$	��255�� �� *CurrentCenterBrightLimit+�� $��X 부호 전환��$��3��$��1���� $��3��$��1�� �� *IsCenterReverseXValue+.���� $��부호 변환���� $��Y 부호 전환��$��4��$��1���� $��4��$��1�� �� *IsCenterReverseYValue+.���� $��부호 변환���� $��6��$��5q����� *SaveCenterSetCommand+{��� *IsChangedCenterParams+.���� $��저장���� $��6��$��1��$��5q����� *RestoreCenterParamsCommand+{��� *IsChangedCenterParams+.���� $��되돌리기���� $��풀컨텐츠��$ 15Q�.��� 
 �� $
��auto ��� �� $
��auto �	 �� �� .+��� $��점수��$��0��$��0��$��Center=�$��5q�  $��0��$��1��$ 0Q�$	 100Q�$��Center=� �� *CurrentFullContentsScore+$   HTool.Template.TemplateGallery  $��2��$��1��   TemplitePath �� * CurrentFullContentsTemplitPath+$ TwoWay=�    
AddCommand  �� *AddTemplitCommand+�� $��5��$��5q����� *SaveFullContentsSetCommand+{��� *IsChangedFullContentsParams+.���� $��저장���� $��6��$��1��$��5q����� *" RestoreFullContentsParamsCommand+{��� *IsChangedFullContentsParams+.���� $��되돌리기��� $��1��.+��� .@��� $��이력��$ 15Q��� $��폴더��$ 15Q�.��� 
 �� $
��auto ��� $
��auto ��� �� $
��auto �.+�� 	 �� $
��auto ��� �� $
��auto �.+��� $��경로��$��0��$��Center=��� $��1��$��Center=����� *SelectedFolderPath+�� $��2������ *LoadFolderImageCommand+.���� $��변경���� $��1�� �� *IsGetSubFile+.���� $'��!하부 디렉토리 포함 여부��&   System.Windows.Controls.DataGrid  $��2��!   AutoGenerateColumns! .  "   CanUserAddRows" .  #   
IsReadOnly# . $   RowDetailsVisibilityMode$$ Visible=����� *ListFolderImage+ �� *SelectedFolderItem+.  (System.Windows.Interactivity.Interaction%   Triggers% /  )System.Windows.Interactivity.EventTrigger  &   	EventName$& MouseDoubleClick��.	  (System.Windows.Interactivity.TriggerBase' 	  Actions.' 6
  0System.Windows.Interactivity.InvokeCommandAction
  ( 
  Command( �� *SelectFolderImageCommand+)   RowDetailsTemplate) �� .R��� $��10q�* �� TextAlignment$* Center=����� *	ResultStr+���� *RowBackgroundColor+%   $& MouseDoubleClick��.' 
  ( �� *SelectFolderImageCommand++   Columns+ 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn,   Header$, 파일 이름��1  +System.Windows.Controls.DataGridBoundColumn-   Binding- �� *Name+�� $��3��$��5q����� *InspectFolderListCommand+.���� $��전체 검사��